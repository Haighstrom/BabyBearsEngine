using System.Threading;
using BabyBearsEngine.AudioSystem;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// Pure-data implementation of <see cref="IPlaylist"/>. Owns the track list, head index, loop /
/// shuffle flags, and the <see cref="TrackChanged"/> event. Delegates the actual playback
/// (binding clip to channel + starting AL) back to its parent service via the constructor
/// callback, so this class stays decoupled from OpenAL.
/// </summary>
internal sealed class MusicPlaylist : IPlaylist
{
    private readonly Lock _lock = new();
    // The bool flag distinguishes user-initiated play (false) from poll-thread auto-advance
    // (true). The service uses this to bail out of auto-advance if the user called StopMusic
    // between the poll detecting end-of-track and the service-side play actually running.
    private readonly Action<IMusicClip, bool> _playTrack;
    private readonly Action _stopMusic;

    private List<IMusicClip> _tracks = [];
    private int _currentIndex = -1;
    // True while a track is actively playing — set when StartTrack fires, cleared when the
    // service tells us via NotifyStopped. Without this, the playlist's "previous track"
    // calculation for TrackChanged keeps returning the now-stopped clip after the user calls
    // StopMusic, so subscribers keying off "Previous != null means we were playing" mis-update.
    private bool _isPlaying = false;
    private bool _loop;
    private bool _shuffle;

    /// <param name="playTrack">Callback invoked when the playlist decides which track to play next. The bool argument is true when the call originates from the poll thread's auto-advance and false when it originates from a user-initiated Play call — the service uses it to drop auto-advance plays that race with a user StopMusic.</param>
    /// <param name="stopMusic">Callback invoked when the playlist transitions to a stopped state (e.g. non-looping playlist reaching its end).</param>
    /// <param name="loop">Initial value of <see cref="Loop"/>.</param>
    /// <param name="shuffle">Initial value of <see cref="Shuffle"/>.</param>
    public MusicPlaylist(Action<IMusicClip, bool> playTrack, Action stopMusic, bool loop, bool shuffle)
    {
        _playTrack = playTrack;
        _stopMusic = stopMusic;
        _loop = loop;
        _shuffle = shuffle;
    }

    public IReadOnlyList<IMusicClip> Tracks
    {
        get
        {
            lock (_lock)
            {
                return _tracks.ToArray();
            }
        }
    }

    public int CurrentIndex
    {
        get
        {
            lock (_lock)
            {
                return _currentIndex;
            }
        }
    }

    public IMusicClip? CurrentTrack
    {
        get
        {
            lock (_lock)
            {
                return _currentIndex >= 0 && _currentIndex < _tracks.Count ? _tracks[_currentIndex] : null;
            }
        }
    }

    public bool Loop
    {
        get
        {
            lock (_lock)
            {
                return _loop;
            }
        }
        set
        {
            lock (_lock)
            {
                _loop = value;
            }
        }
    }

    public bool Shuffle
    {
        get
        {
            lock (_lock)
            {
                return _shuffle;
            }
        }
        set
        {
            lock (_lock)
            {
                _shuffle = value;
            }
        }
    }

    public event Action<MusicTrackChangedEventArgs>? TrackChanged;

    public void SetTracks(IReadOnlyList<IMusicClip> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        lock (_lock)
        {
            _tracks = [.. tracks];
            _currentIndex = -1;
        }
    }

    public void SetTracks(params IMusicClip[] tracks) => SetTracks((IReadOnlyList<IMusicClip>)tracks);

    public void Clear()
    {
        lock (_lock)
        {
            _tracks.Clear();
            _currentIndex = -1;
            _isPlaying = false;
        }
        _stopMusic();
    }

    public void Play()
    {
        IMusicClip? toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            if (_tracks.Count == 0)
            {
                return;
            }

            if (_shuffle)
            {
                ShuffleTracks();
            }

            previous = PreviousForChangeEventUnlocked();
            _currentIndex = 0;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    public void Play(int index)
    {
        IMusicClip toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            if (_tracks.Count == 0)
            {
                return;
            }

            previous = PreviousForChangeEventUnlocked();
            int wrapped = ((index % _tracks.Count) + _tracks.Count) % _tracks.Count;
            _currentIndex = wrapped;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    public void Play(string trackName)
    {
        ArgumentNullException.ThrowIfNull(trackName);
        IMusicClip toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            int index = -1;
            for (int i = 0; i < _tracks.Count; i++)
            {
                if (string.Equals(_tracks[i].TrackName, trackName, StringComparison.OrdinalIgnoreCase))
                {
                    index = i;
                    break;
                }
            }

            if (index < 0)
            {
                throw new KeyNotFoundException($"No track named '{trackName}' in the playlist.");
            }

            previous = PreviousForChangeEventUnlocked();
            _currentIndex = index;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    public void Play(IMusicClip clip)
    {
        ArgumentNullException.ThrowIfNull(clip);
        IMusicClip toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            int index = _tracks.IndexOf(clip);
            if (index < 0)
            {
                throw new ArgumentException("Clip is not present in the playlist; add it via SetTracks first.", nameof(clip));
            }

            previous = PreviousForChangeEventUnlocked();
            _currentIndex = index;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    public void NextTrack()
    {
        IMusicClip toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            if (_tracks.Count == 0)
            {
                return;
            }

            previous = PreviousForChangeEventUnlocked();
            _currentIndex = _currentIndex < 0 ? 0 : (_currentIndex + 1) % _tracks.Count;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    public void PreviousTrack()
    {
        IMusicClip toPlay;
        IMusicClip? previous;
        lock (_lock)
        {
            if (_tracks.Count == 0)
            {
                return;
            }

            previous = PreviousForChangeEventUnlocked();
            _currentIndex = _currentIndex <= 0 ? _tracks.Count - 1 : _currentIndex - 1;
            toPlay = _tracks[_currentIndex];
        }
        StartTrack(toPlay, previous);
    }

    /// <summary>
    /// Called by the audio service's polling thread when the current music track finishes naturally.
    /// Advances to the next track if <see cref="Loop"/> permits; otherwise stops.
    /// </summary>
    public void OnTrackFinished()
    {
        IMusicClip? toPlay = null;
        IMusicClip? previous = null;
        bool shouldStop = false;
        lock (_lock)
        {
            if (_tracks.Count == 0)
            {
                return;
            }

            previous = PreviousForChangeEventUnlocked();
            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _tracks.Count)
            {
                if (!_loop)
                {
                    _currentIndex = -1;
                    _isPlaying = false;
                    shouldStop = true;
                }
                else
                {
                    if (_shuffle)
                    {
                        // Reshuffle for the next pass. If the new front of the playlist matches the
                        // track that just ended, reshuffle again so listeners don't hear it twice in
                        // a row. With Count > 1 we're guaranteed a shuffle exists that doesn't lead
                        // with `previous`.
                        ShuffleTracks();
                        while (_tracks.Count > 1 && previous is not null && ReferenceEquals(_tracks[0], previous))
                        {
                            ShuffleTracks();
                        }
                    }
                    _currentIndex = 0;
                    toPlay = _tracks[_currentIndex];
                }
            }
            else
            {
                _currentIndex = nextIndex;
                toPlay = _tracks[_currentIndex];
            }
        }

        if (shouldStop)
        {
            _stopMusic();
        }
        else if (toPlay is not null)
        {
            StartTrack(toPlay, previous, autoAdvance: true);
        }
    }

    private void StartTrack(IMusicClip toPlay, IMusicClip? previous, bool autoAdvance = false)
    {
        // Mark playing BEFORE the callback — _playTrack synchronously triggers a state
        // transition that subscribers may key off, and they should see the playlist as live by
        // then. lock-free safe: only the calling thread touches _isPlaying here.
        lock (_lock)
        {
            _isPlaying = true;
        }
        _playTrack(toPlay, autoAdvance);
        TrackChanged?.Invoke(new MusicTrackChangedEventArgs(previous, toPlay));
    }

    /// <summary>
    /// Called by the audio service after an external <c>StopMusic</c> so the playlist's idea of
    /// "previous track" for the next <see cref="TrackChanged"/> can correctly say "nothing was
    /// playing" rather than the now-stopped clip.
    /// </summary>
    internal void NotifyStopped()
    {
        lock (_lock)
        {
            _isPlaying = false;
        }
    }

    private IMusicClip? CurrentTrackUnlocked()
        => _currentIndex >= 0 && _currentIndex < _tracks.Count ? _tracks[_currentIndex] : null;

    /// <summary>Returns the current track only when a track is actually playing; null otherwise (e.g. just after StopMusic). Used for the <c>Previous</c> field of <see cref="MusicTrackChangedEventArgs"/>.</summary>
    private IMusicClip? PreviousForChangeEventUnlocked()
        => _isPlaying ? CurrentTrackUnlocked() : null;

    private void ShuffleTracks()
    {
        // _lock is held by the caller. Routes through the engine-wide IRandom so tests can
        // substitute a deterministic source via EngineConfiguration.RandomService.
        Randomisation.Shuffle(_tracks);
    }
}

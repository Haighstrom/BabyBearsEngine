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
    private readonly Random _random;
    private readonly Action<IMusicClip> _playTrack;
    private readonly Action _stopMusic;

    private List<IMusicClip> _tracks = [];
    private int _currentIndex = -1;
    private bool _loop;
    private bool _shuffle;

    /// <param name="playTrack">Callback invoked when the playlist decides which track to play next. The callback is responsible for actually starting AL playback.</param>
    /// <param name="stopMusic">Callback invoked when the playlist transitions to a stopped state (e.g. non-looping playlist reaching its end).</param>
    /// <param name="loop">Initial value of <see cref="Loop"/>.</param>
    /// <param name="shuffle">Initial value of <see cref="Shuffle"/>.</param>
    public MusicPlaylist(Action<IMusicClip> playTrack, Action stopMusic, bool loop, bool shuffle)
    {
        _playTrack = playTrack;
        _stopMusic = stopMusic;
        _loop = loop;
        _shuffle = shuffle;
        _random = new Random();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
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

            previous = CurrentTrackUnlocked();
            int nextIndex = _currentIndex + 1;
            if (nextIndex >= _tracks.Count)
            {
                if (!_loop)
                {
                    _currentIndex = -1;
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
            StartTrack(toPlay, previous);
        }
    }

    private void StartTrack(IMusicClip toPlay, IMusicClip? previous)
    {
        _playTrack(toPlay);
        TrackChanged?.Invoke(new MusicTrackChangedEventArgs(previous, toPlay));
    }

    private IMusicClip? CurrentTrackUnlocked()
        => _currentIndex >= 0 && _currentIndex < _tracks.Count ? _tracks[_currentIndex] : null;

    private void ShuffleTracks()
    {
        // Fisher-Yates. _lock is held by the caller.
        for (int i = _tracks.Count - 1; i > 0; i--)
        {
            int swap = _random.Next(i + 1);
            (_tracks[i], _tracks[swap]) = (_tracks[swap], _tracks[i]);
        }
    }
}

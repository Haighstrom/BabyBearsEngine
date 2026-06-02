using System.IO;
using System.Threading;
using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Diagnostics;
using OpenTK.Audio.OpenAL;

namespace BabyBearsEngine.Platform.OpenAL;

/// <summary>
/// OpenAL-backed implementation of <see cref="IAudio"/>. Owns the device + context, the music
/// channel, the SFX channel pool, the music playlist, and a background polling thread that detects
/// when the current music track has finished and advances the playlist.
///
/// All AL calls go through OpenAL Soft's globally-current context, so any thread can issue them
/// once the context has been made current at startup. Per-channel races (e.g. SFX start mid-update)
/// are guarded by <see cref="_channelLock"/>.
///
/// If initialisation fails (no device, missing DLL) or <see cref="AudioSettings.Disabled"/> is set,
/// the service runs in unavailable mode: <see cref="IsAvailable"/> is false and every play / load
/// call no-ops. Volumes still store-and-recall and the playlist still works as a data structure so
/// game code can build it up regardless.
/// </summary>
internal sealed class OpenALAudioService : IAudio
{
    private static readonly TimeSpan s_pollInterval = TimeSpan.FromMilliseconds(50);

    private readonly Lock _channelLock = new();
    private readonly Lock _volumeLock = new();
    private readonly List<OpenALAudioClip> _ownedClips = [];

    private readonly ALDevice _device;
    private readonly ALContext _context;
    private readonly bool _initialised;

    private readonly OpenALAudioChannel? _musicChannel;
    private readonly OpenALAudioChannel[] _sfxChannels;
    private readonly MusicPlaylist _playlist;

    private readonly Thread? _pollThread;
    private readonly CancellationTokenSource? _pollCts;

    private float _masterVolume;
    private float _musicVolume;
    private float _sfxVolume;
    private AudioState _musicState = AudioState.Stopped;
    private bool _paused = false;
    // Two flags rather than one: _disposing is set immediately under the lock so the poll
    // thread, PlayMusicClipInternal, and other code paths see the shutdown in progress and
    // bail out before touching AL resources mid-teardown. _disposed is set only after a full
    // successful teardown, so that if anything throws during Dispose a retry can attempt the
    // cleanup again instead of silently leaking the device/context/sources/buffers.
    private bool _disposing = false;
    private bool _disposed = false;

    public OpenALAudioService(AudioSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        _masterVolume = Clamp01(settings.MasterVolume);
        _musicVolume = Clamp01(settings.MusicVolume);
        _sfxVolume = Clamp01(settings.SfxVolume);
        _sfxChannels = [];

        _playlist = new MusicPlaylist(
            playTrack: (clip, autoAdvance) => PlayMusicClipInternal(clip, autoAdvance),
            stopMusic: () => StopMusic(),
            loop: settings.LoopMusic,
            shuffle: settings.ShuffleMusic);

        if (settings.Disabled)
        {
            Logger.Information("Audio: subsystem disabled via AudioSettings.Disabled. All audio calls will no-op.");
            return;
        }

        try
        {
            _device = ALC.OpenDevice(null);
            if (_device == ALDevice.Null)
            {
                Logger.Warning("Audio: no OpenAL device available; running in no-op mode. " +
                    "Install OpenAL Soft (ship OpenAL32.dll alongside the executable) to enable audio.");
                return;
            }

            _context = ALC.CreateContext(_device, (int[]?)null);
            if (_context == ALContext.Null)
            {
                Logger.Warning("Audio: failed to create OpenAL context; running in no-op mode.");
                ALC.CloseDevice(_device);
                _device = ALDevice.Null;
                return;
            }

            ALC.MakeContextCurrent(_context);

            _musicChannel = new OpenALAudioChannel();
            _sfxChannels = new OpenALAudioChannel[settings.MaxSfxChannels];
            for (int i = 0; i < settings.MaxSfxChannels; i++)
            {
                _sfxChannels[i] = new OpenALAudioChannel();
            }

            _initialised = true;
            Logger.Information($"Audio: OpenAL initialised with {settings.MaxSfxChannels} SFX channels.");

            _pollCts = new CancellationTokenSource();
            _pollThread = new Thread(PollLoop)
            {
                Name = "BBE Audio Poll",
                IsBackground = true,
            };
            _pollThread.Start();
        }
        catch (Exception ex)
        {
            System.Text.StringBuilder detail = new();
            Exception? current = ex;
            while (current is not null)
            {
                if (detail.Length > 0)
                {
                    detail.Append(" -> ");
                }
                detail.Append(current.GetType().Name).Append(": ").Append(current.Message);
                current = current.InnerException;
            }
            Logger.Warning($"Audio: OpenAL initialisation failed ({detail}); running in no-op mode.");
            _initialised = false;
        }
    }

    public bool IsAvailable => _initialised;

    public AudioState MusicState
    {
        get
        {
            lock (_channelLock)
            {
                return _musicState;
            }
        }
    }

    public IPlaylist Playlist => _playlist;

    public float MasterVolume
    {
        get
        {
            lock (_volumeLock)
            {
                return _masterVolume;
            }
        }
        set
        {
            lock (_volumeLock)
            {
                _masterVolume = Clamp01(value);
            }
            ApplyVolumes();
        }
    }

    public float MusicVolume
    {
        get
        {
            lock (_volumeLock)
            {
                return _musicVolume;
            }
        }
        set
        {
            lock (_volumeLock)
            {
                _musicVolume = Clamp01(value);
            }
            ApplyVolumes();
        }
    }

    public float SfxVolume
    {
        get
        {
            lock (_volumeLock)
            {
                return _sfxVolume;
            }
        }
        set
        {
            lock (_volumeLock)
            {
                _sfxVolume = Clamp01(value);
            }
            ApplyVolumes();
        }
    }

    public event Action<AudioStateChangedEventArgs>? MusicStateChanged;

    public IMusicClip LoadMusic(string path) => LoadMusic(path, Path.GetFileNameWithoutExtension(path));

    public IMusicClip LoadMusic(string path, string trackName)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentNullException.ThrowIfNull(trackName);

        if (!_initialised)
        {
            return new NoOpMusicClip(path, trackName);
        }

        DecodedAudio decoded = DecodeByExtension(path);
        OpenALMusicClip clip;
        lock (_channelLock)
        {
            clip = new OpenALMusicClip(path, trackName, decoded);
            _ownedClips.Add(clip);
        }
        return clip;
    }

    public ISfxClip LoadSfx(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        if (!_initialised)
        {
            return new NoOpSfxClip(path);
        }

        DecodedAudio decoded = DecodeByExtension(path);
        OpenALSfxClip clip;
        lock (_channelLock)
        {
            clip = new OpenALSfxClip(path, decoded);
            _ownedClips.Add(clip);
        }
        return clip;
    }

    public void PlayMusic(IMusicClip clip)
    {
        ArgumentNullException.ThrowIfNull(clip);
        _playlist.SetTracks(clip);
        _playlist.Play();
    }

    public void PlayMusic(IReadOnlyList<IMusicClip> playlist)
    {
        ArgumentNullException.ThrowIfNull(playlist);
        _playlist.SetTracks(playlist);
        _playlist.Play();
    }

    public void PlaySfx(ISfxClip clip)
    {
        ArgumentNullException.ThrowIfNull(clip);
        PlaySfxInternal(clip, volumeOverride: null);
    }

    public void PlaySfx(ISfxClip clip, float volumeOverride)
    {
        ArgumentNullException.ThrowIfNull(clip);
        PlaySfxInternal(clip, Clamp01(volumeOverride));
    }

    public void Pause()
    {
        if (!_initialised)
        {
            return;
        }

        lock (_channelLock)
        {
            if (_paused)
            {
                return;
            }

            // Music pauses (so it resumes from the same point), but SFX are stopped — pausing
            // transient one-shot SFX would cause them all to restart simultaneously on Resume,
            // mixed with whatever the game is doing at that point. Stop is the standard policy.
            _musicChannel?.Pause();
            foreach (OpenALAudioChannel channel in _sfxChannels)
            {
                channel.Stop();
            }
            _paused = true;
            TransitionMusicStateUnlocked(_musicState == AudioState.Playing ? AudioState.Paused : _musicState);
        }
    }

    public void Resume()
    {
        if (!_initialised)
        {
            return;
        }

        lock (_channelLock)
        {
            if (!_paused)
            {
                return;
            }

            // SFX were stopped on Pause, not paused — there's nothing to resume on those channels.
            _musicChannel?.Resume();
            _paused = false;
            TransitionMusicStateUnlocked(_musicState == AudioState.Paused ? AudioState.Playing : _musicState);
        }
    }

    public void StopMusic()
    {
        if (!_initialised)
        {
            return;
        }

        lock (_channelLock)
        {
            _musicChannel?.Stop();
            TransitionMusicStateUnlocked(AudioState.Stopped);
        }
        // Tell the playlist so its next TrackChanged event reports Previous = null rather than
        // the now-stopped clip. Done outside the channel lock — playlist has its own lock.
        _playlist.NotifyStopped();
    }

    public void StopSfx()
    {
        if (!_initialised)
        {
            return;
        }

        lock (_channelLock)
        {
            foreach (OpenALAudioChannel channel in _sfxChannels)
            {
                channel.Stop();
            }
        }
    }

    public void StopAll()
    {
        StopMusic();
        StopSfx();
    }

    public void Dispose()
    {
        // Mark "disposing" under the lock so the poll thread (next time it acquires the lock
        // for CheckMusicChannelForAdvancement) sees it and bails before touching channels, and
        // so any concurrent PlayMusicClipInternal call won't kick off new playback after the
        // teardown started.
        lock (_channelLock)
        {
            if (_disposed || _disposing)
            {
                return;
            }
            _disposing = true;
        }

        _pollCts?.Cancel();
        try
        {
            // No timeout — the poll thread sleeps for ~50ms between ticks so cancellation is
            // already responsive, and we'd rather block briefly than race the thread by
            // proceeding with teardown while it may still be in CheckMusicChannelForAdvancement.
            _pollThread?.Join();
        }
        catch
        {
            // Best-effort shutdown — we'll proceed to tear down the context regardless.
        }
        _pollCts?.Dispose();

        try
        {
            if (_initialised)
            {
                lock (_channelLock)
                {
                    TryAl(() => _musicChannel?.Stop());
                    foreach (OpenALAudioChannel channel in _sfxChannels)
                    {
                        TryAl(channel.Stop);
                    }

                    foreach (OpenALAudioClip clip in _ownedClips)
                    {
                        TryAl(clip.Destroy);
                    }
                    _ownedClips.Clear();

                    TryAl(() => _musicChannel?.Dispose());
                    foreach (OpenALAudioChannel channel in _sfxChannels)
                    {
                        TryAl(channel.Dispose);
                    }
                }

                TryAl(() => ALC.MakeContextCurrent(ALContext.Null));
                if (_context != ALContext.Null)
                {
                    TryAl(() => ALC.DestroyContext(_context));
                }
                if (_device != ALDevice.Null)
                {
                    TryAl(() => ALC.CloseDevice(_device));
                }
            }

            _disposed = true;
        }
        finally
        {
            // Clear _disposing whether or not we succeeded — if anything threw above, _disposed
            // stays false and a caller can retry. If everything succeeded, _disposed is true
            // and the next Dispose call short-circuits at the top.
            _disposing = false;
        }
    }

    private static void TryAl(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Logger.Warning($"Audio: AL teardown step failed ({ex.GetType().Name}: {ex.Message}); continuing.");
        }
    }

    private void PlayMusicClipInternal(IMusicClip clip, bool autoAdvance = false)
    {
        if (!_initialised || _musicChannel is null)
        {
            return;
        }

        // Bonus safety: if a fake or external clip somehow arrived, just no-op. Real music clips
        // produced by our LoadMusic always satisfy this.
        if (clip is not OpenALMusicClip openAlClip)
        {
            Logger.Warning($"Audio: ignoring PlayMusic for clip '{clip.Path}' — clip was not produced by the audio service.");
            return;
        }

        lock (_channelLock)
        {
            // Dispose started after the poll thread queued an OnTrackFinished — bail before
            // touching the music channel (which is about to be / has been torn down).
            if (_disposing || _disposed)
            {
                return;
            }
            // Auto-advance from the poll thread loses to a concurrent user StopMusic: if the
            // user has already set the state to Stopped between the poll detecting end-of-track
            // and this call, honour their intent rather than restarting playback.
            if (autoAdvance && _musicState != AudioState.Playing)
            {
                return;
            }
            _musicChannel.Play(openAlClip, ComputeMusicGainUnlocked());
            _paused = false;
            TransitionMusicStateUnlocked(AudioState.Playing);
        }
    }

    private void PlaySfxInternal(ISfxClip clip, float? volumeOverride)
    {
        if (!_initialised)
        {
            return;
        }

        if (clip is not OpenALSfxClip openAlClip)
        {
            Logger.Warning($"Audio: ignoring PlaySfx for clip '{clip.Path}' — clip was not produced by the audio service.");
            return;
        }

        lock (_channelLock)
        {
            foreach (OpenALAudioChannel channel in _sfxChannels)
            {
                if (channel.IsFree)
                {
                    float gain = ComputeSfxGainUnlocked();
                    if (volumeOverride.HasValue)
                    {
                        gain *= volumeOverride.Value;
                    }
                    channel.Play(openAlClip, gain);
                    return;
                }
            }
        }

        Logger.Warning($"Audio: all SFX channels busy; dropped '{clip.Path}'. Consider increasing AudioSettings.MaxSfxChannels.");
    }

    private DecodedAudio DecodeByExtension(string path)
    {
        string ext = Path.GetExtension(path).ToLowerInvariant();
        return ext switch
        {
            ".wav" => WavDecoder.Decode(path),
            ".ogg" => OggDecoder.Decode(path),
            ".mp3" => Mp3Decoder.Decode(path),
            ".flac" => FlacDecoder.Decode(path),
            ".opus" => OpusDecoder.Decode(path),
            _ => throw new NotSupportedException(
                $"Audio: file extension '{ext}' is not supported. Supported formats: .wav, .ogg, .mp3, .flac, .opus. " +
                "MIDI (.mid/.midi) support is tracked at GitHub issue #121."),
        };
    }

    private void ApplyVolumes()
    {
        if (!_initialised)
        {
            return;
        }

        float musicGain = ComputeMusicGainUnlocked();
        float sfxGain = ComputeSfxGainUnlocked();

        lock (_channelLock)
        {
            _musicChannel?.SetGain(musicGain);
            foreach (OpenALAudioChannel channel in _sfxChannels)
            {
                channel.SetGain(sfxGain);
            }
        }
    }

    private float ComputeMusicGainUnlocked()
    {
        lock (_volumeLock)
        {
            return _masterVolume * _musicVolume;
        }
    }

    private float ComputeSfxGainUnlocked()
    {
        lock (_volumeLock)
        {
            return _masterVolume * _sfxVolume;
        }
    }

    private void TransitionMusicStateUnlocked(AudioState newState)
    {
        if (_musicState == newState)
        {
            return;
        }

        AudioState old = _musicState;
        _musicState = newState;
        MusicStateChanged?.Invoke(new AudioStateChangedEventArgs(old, newState));
    }

    private void PollLoop()
    {
        CancellationToken token = _pollCts!.Token;
        while (!token.IsCancellationRequested)
        {
            try
            {
                CheckMusicChannelForAdvancement();
            }
            catch (Exception ex)
            {
                Logger.Warning($"Audio: poll loop tick failed ({ex.GetType().Name}: {ex.Message}).");
            }

            try
            {
                Thread.Sleep(s_pollInterval);
            }
            catch (ThreadInterruptedException)
            {
                break;
            }
        }
    }

    private void CheckMusicChannelForAdvancement()
    {
        bool finished;
        lock (_channelLock)
        {
            // Bail out if Dispose has started — _musicChannel and clips may be torn down at any
            // moment from here on, so we must not call OnTrackFinished (which would re-enter
            // PlayMusicClipInternal and touch them).
            if (_disposing || _disposed || _musicChannel is null || _paused || _musicState != AudioState.Playing)
            {
                return;
            }
            finished = _musicChannel.HasFinished;
        }

        if (finished)
        {
            // OnTrackFinished may call back into PlayMusicClipInternal or StopMusic, both of which
            // take _channelLock — so we release the lock around it to avoid re-entry.
            _playlist.OnTrackFinished();
        }
    }

    private static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }
        if (value > 1f)
        {
            return 1f;
        }
        return value;
    }
}

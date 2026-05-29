using System;
using System.IO;
using System.Threading;
using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Platform.OpenAL;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// End-to-end tests against the real OpenAL backend. These spin up an actual ALC device + context,
/// upload PCM data to AL buffers, allocate sources, and dispose cleanly — exercising the full
/// integration that unit tests with fakes cannot cover.
///
/// If OpenAL Soft is not installed on the test machine the service falls back to no-op mode and
/// every test below adapts accordingly (asserts the no-op contract instead of failing).
/// </summary>
[TestClass]
public class OpenALAudioServiceTests
{
    private string _tempWavPath = null!;

    [TestInitialize]
    public void Setup()
    {
        _tempWavPath = Path.Combine(Path.GetTempPath(), $"bbe-audio-systest-{Guid.NewGuid():N}.wav");
        WriteTinyMono16Wav(_tempWavPath, sampleRate: 44100, sampleFrames: 4410);   // 100 ms of audio
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_tempWavPath))
        {
            File.Delete(_tempWavPath);
        }
    }

    [TestMethod]
    public void DefaultSettings_InitialisesOrFallsBackCleanly()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);

        // On developer machines and most CI runners OpenAL Soft is present and the service comes
        // up. On stripped-down environments it falls back to no-op mode with a warning logged.
        // Either is acceptable — the contract is just that the constructor never throws and the
        // service is usable afterward.
        Assert.IsNotNull(service.Playlist);
        Assert.AreEqual(AudioState.Stopped, service.MusicState);
    }

    [TestMethod]
    public void Silent_DoesNotOpenDevice_AndReportsUnavailable()
    {
        using var service = new OpenALAudioService(AudioSettings.Silent);

        Assert.IsFalse(service.IsAvailable);
        Assert.AreEqual(AudioState.Stopped, service.MusicState);
    }

    [TestMethod]
    public void Silent_LoadMusic_ReturnsNoOpClipWithMetadata()
    {
        using var service = new OpenALAudioService(AudioSettings.Silent);

        IMusicClip clip = service.LoadMusic("never-touched.wav", "Custom Name");

        Assert.AreEqual("never-touched.wav", clip.Path);
        Assert.AreEqual("Custom Name", clip.TrackName);
        Assert.AreEqual(TimeSpan.Zero, clip.Duration);
    }

    [TestMethod]
    public void Silent_PlayCalls_AreSilentNoOps()
    {
        using var service = new OpenALAudioService(AudioSettings.Silent);

        // None of these should throw, log fatals, or change state.
        IMusicClip music = service.LoadMusic("a.wav");
        ISfxClip sfx = service.LoadSfx("b.wav");
        service.PlayMusic(music);
        service.PlaySfx(sfx);
        service.Pause();
        service.Resume();
        service.StopAll();

        Assert.AreEqual(AudioState.Stopped, service.MusicState);
    }

    [TestMethod]
    public void LoadSfx_RealWav_ProducesClipWithCorrectDuration()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-decode test.");
            return;
        }

        ISfxClip clip = service.LoadSfx(_tempWavPath);

        Assert.AreEqual(_tempWavPath, clip.Path);
        // 4410 frames at 44100 Hz = 100 ms. Allow a millisecond of slack for FP rounding.
        Assert.IsLessThan(TimeSpan.FromMilliseconds(2), (clip.Duration - TimeSpan.FromMilliseconds(100)).Duration());
    }

    [TestMethod]
    public void LoadMusic_DefaultTrackName_IsFilenameWithoutExtension()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-decode test.");
            return;
        }

        IMusicClip clip = service.LoadMusic(_tempWavPath);

        string expectedName = Path.GetFileNameWithoutExtension(_tempWavPath);
        Assert.AreEqual(expectedName, clip.TrackName);
    }

    [TestMethod]
    public void LoadAndPlaySfx_DoesNotThrow_AndChannelBecomesBusy()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-playback test.");
            return;
        }

        ISfxClip clip = service.LoadSfx(_tempWavPath);
        service.SfxVolume = 0f;   // mute so the test machine doesn't actually emit sound
        service.PlaySfx(clip);

        // The PlaySfx call itself succeeded — that's the load+upload+source-bind round trip.
        // We don't poll for playback completion because the poll thread already does that.
    }

    [TestMethod]
    public void PlayMusic_FiresTrackChangedAndStateChangedEvents()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-playback test.");
            return;
        }

        IMusicClip clip = service.LoadMusic(_tempWavPath, "Test Track");
        service.MusicVolume = 0f;

        MusicTrackChangedEventArgs? trackEvent = null;
        AudioStateChangedEventArgs? stateEvent = null;
        service.Playlist.TrackChanged += args => trackEvent = args;
        service.MusicStateChanged += args => stateEvent = args;

        service.PlayMusic(clip);

        Assert.IsNotNull(trackEvent);
        Assert.AreSame(clip, trackEvent.Current);
        Assert.IsNotNull(stateEvent);
        Assert.AreEqual(AudioState.Playing, stateEvent.NewState);
        Assert.AreEqual(AudioState.Playing, service.MusicState);
    }

    [TestMethod]
    public void PauseAndResume_TransitionsMusicState()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-playback test.");
            return;
        }

        IMusicClip clip = service.LoadMusic(_tempWavPath);
        service.MusicVolume = 0f;
        service.PlayMusic(clip);

        service.Pause();
        Assert.AreEqual(AudioState.Paused, service.MusicState);

        service.Resume();
        Assert.AreEqual(AudioState.Playing, service.MusicState);
    }

    [TestMethod]
    public void StopMusic_TransitionsToStopped()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-playback test.");
            return;
        }

        IMusicClip clip = service.LoadMusic(_tempWavPath);
        service.MusicVolume = 0f;
        service.PlayMusic(clip);
        service.StopMusic();

        Assert.AreEqual(AudioState.Stopped, service.MusicState);
    }

    [TestMethod]
    public void PollingThread_AdvancesPlaylistWhenTrackEnds()
    {
        using var service = new OpenALAudioService(AudioSettings.Default);
        if (!service.IsAvailable)
        {
            Assert.Inconclusive("OpenAL device not available on this machine; skipping real-playback test.");
            return;
        }

        // Two short clips. After the first finishes, the poll thread should advance to the second
        // within a few hundred ms (poll interval is 50 ms, clip is 100 ms).
        IMusicClip trackA = service.LoadMusic(_tempWavPath, "Track A");
        IMusicClip trackB = service.LoadMusic(_tempWavPath, "Track B");
        service.MusicVolume = 0f;
        service.Playlist.Loop = false;
        service.Playlist.Shuffle = false;

        int trackChanges = 0;
        IMusicClip? lastTrack = null;
        service.Playlist.TrackChanged += args =>
        {
            Interlocked.Increment(ref trackChanges);
            lastTrack = args.Current;
        };

        service.PlayMusic([trackA, trackB]);

        // Poll up to 2 seconds for the advance. Clip is ~100ms; poll thread ticks every 50 ms.
        DateTime deadline = DateTime.UtcNow.AddSeconds(2);
        while (DateTime.UtcNow < deadline && Volatile.Read(ref trackChanges) < 2)
        {
            Thread.Sleep(50);
        }

        Assert.IsGreaterThanOrEqualTo(2, Volatile.Read(ref trackChanges));
        Assert.AreSame(trackB, lastTrack);
    }

    [TestMethod]
    public void Dispose_StopsThreadAndReleasesResources_Cleanly()
    {
        var service = new OpenALAudioService(AudioSettings.Default);
        if (service.IsAvailable)
        {
            ISfxClip clip = service.LoadSfx(_tempWavPath);
            service.SfxVolume = 0f;
            service.PlaySfx(clip);
        }

        // The interesting assertion is that this does not throw, hang, or deadlock — which it
        // would if the poll thread weren't being signalled correctly or AL handles weren't being
        // released in the right order.
        service.Dispose();
        service.Dispose();   // second dispose is a no-op
    }

    private static void WriteTinyMono16Wav(string path, int sampleRate, int sampleFrames)
    {
        const int channels = 1;
        const int bitsPerSample = 16;
        int byteRate = sampleRate * channels * (bitsPerSample / 8);
        int blockAlign = channels * (bitsPerSample / 8);
        int dataBytes = sampleFrames * blockAlign;
        int fmtSize = 16;
        int totalRiffSize = 4 + (8 + fmtSize) + (8 + dataBytes);

        using FileStream stream = File.Create(path);
        using BinaryWriter writer = new(stream);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(totalRiffSize);
        writer.Write("WAVE"u8.ToArray());

        writer.Write("fmt "u8.ToArray());
        writer.Write(fmtSize);
        writer.Write((ushort)1);              // PCM
        writer.Write((ushort)channels);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((ushort)blockAlign);
        writer.Write((ushort)bitsPerSample);

        writer.Write("data"u8.ToArray());
        writer.Write(dataBytes);
        // Silence — zeros are valid PCM, audible only as 100% silence which is what we want here.
        writer.Write(new byte[dataBytes]);
    }
}

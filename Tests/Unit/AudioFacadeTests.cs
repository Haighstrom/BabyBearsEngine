using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.AudioSystem;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AudioFacadeTests
{
    private sealed class FakePlaylist : IPlaylist
    {
        public IReadOnlyList<IMusicClip> Tracks { get; set; } = [];
        public int CurrentIndex { get; set; } = -1;
        public IMusicClip? CurrentTrack { get; set; }
        public bool Loop { get; set; } = true;
        public bool Shuffle { get; set; } = false;
        public void SetTracks(IReadOnlyList<IMusicClip> tracks) => Tracks = tracks;
        public void SetTracks(params IMusicClip[] tracks) => Tracks = tracks;
        public void Clear() => Tracks = [];
        public void Play() { }
        public void Play(int index) { }
        public void Play(string trackName) { }
        public void Play(IMusicClip clip) { }
        public void NextTrack() { }
        public void PreviousTrack() { }
        public event Action<MusicTrackChangedEventArgs>? TrackChanged;
    }

    private sealed class FakeMusicClip : IMusicClip
    {
        public string Path => "fake.wav";
        public TimeSpan Duration => TimeSpan.Zero;
        public string TrackName => "Fake";
        public void Dispose() { }
    }

    private sealed class FakeSfxClip : ISfxClip
    {
        public string Path => "fake-sfx.wav";
        public TimeSpan Duration => TimeSpan.Zero;
        public void Dispose() { }
    }

    private sealed class FakeAudio : IAudio
    {
        public List<(string Method, object? Arg)> Calls { get; } = [];

        public bool IsAvailable { get; set; } = true;
        public AudioState MusicState { get; set; } = AudioState.Stopped;
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;
        public IPlaylist Playlist { get; } = new FakePlaylist();

        public IMusicClip LoadMusic(string path) { Calls.Add(("LoadMusic", path)); return new FakeMusicClip(); }
        public IMusicClip LoadMusic(string path, string trackName) { Calls.Add(("LoadMusicWithName", (path, trackName))); return new FakeMusicClip(); }
        public ISfxClip LoadSfx(string path) { Calls.Add(("LoadSfx", path)); return new FakeSfxClip(); }
        public void PlayMusic(IMusicClip clip) => Calls.Add(("PlayMusicClip", clip));
        public void PlayMusic(IReadOnlyList<IMusicClip> playlist) => Calls.Add(("PlayMusicList", playlist));
        public void PlaySfx(ISfxClip clip) => Calls.Add(("PlaySfx", clip));
        public void PlaySfx(ISfxClip clip, float volumeOverride) => Calls.Add(("PlaySfxWithVolume", (clip, volumeOverride)));
        public void Pause() => Calls.Add(("Pause", null));
        public void Resume() => Calls.Add(("Resume", null));
        public void StopMusic() => Calls.Add(("StopMusic", null));
        public void StopSfx() => Calls.Add(("StopSfx", null));
        public void StopAll() => Calls.Add(("StopAll", null));
        public void Dispose() { }
        public event Action<AudioStateChangedEventArgs>? MusicStateChanged;
        public void RaiseStateChanged(AudioStateChangedEventArgs args) => MusicStateChanged?.Invoke(args);
    }

    private FakeAudio _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeAudio();
        EngineConfiguration.AudioService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void Member_BeforeServiceInstalled_Throws()
    {
        EngineConfiguration.Reset();
        Assert.ThrowsExactly<InvalidOperationException>(() => Audio.PlaySfx(new FakeSfxClip()));
    }

    [TestMethod]
    public void IsAvailable_ProxiesService()
    {
        _fake.IsAvailable = false;
        Assert.IsFalse(Audio.IsAvailable);
        _fake.IsAvailable = true;
        Assert.IsTrue(Audio.IsAvailable);
    }

    [TestMethod]
    public void MusicState_ProxiesService()
    {
        _fake.MusicState = AudioState.Playing;
        Assert.AreEqual(AudioState.Playing, Audio.MusicState);
    }

    [TestMethod]
    public void Playlist_ReturnsServicePlaylist()
    {
        Assert.AreSame(_fake.Playlist, Audio.Playlist);
    }

    [TestMethod]
    public void VolumeProperties_ProxyService()
    {
        Audio.MasterVolume = 0.5f;
        Audio.MusicVolume = 0.6f;
        Audio.SfxVolume = 0.7f;

        Assert.AreEqual(0.5f, _fake.MasterVolume);
        Assert.AreEqual(0.6f, _fake.MusicVolume);
        Assert.AreEqual(0.7f, _fake.SfxVolume);
    }

    [TestMethod]
    public void LoadMusic_PathOnly_RoutesToPathOverload()
    {
        Audio.LoadMusic("song.wav");
        Assert.AreEqual(("LoadMusic", (object?)"song.wav"), _fake.Calls.Single());
    }

    [TestMethod]
    public void LoadMusic_PathAndName_RoutesToNamedOverload()
    {
        Audio.LoadMusic("song.wav", "Theme");
        Assert.AreEqual("LoadMusicWithName", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void LoadSfx_RoutesToService()
    {
        Audio.LoadSfx("bang.wav");
        Assert.AreEqual(("LoadSfx", (object?)"bang.wav"), _fake.Calls.Single());
    }

    [TestMethod]
    public void PlayMusic_SingleClip_RoutesToClipOverload()
    {
        var clip = new FakeMusicClip();
        Audio.PlayMusic(clip);
        Assert.AreEqual("PlayMusicClip", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void PlayMusic_Params_RoutesToListOverload()
    {
        var a = new FakeMusicClip();
        var b = new FakeMusicClip();
        Audio.PlayMusic(a, b);
        Assert.AreEqual("PlayMusicList", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void PlaySfx_NoVolume_RoutesToBasicOverload()
    {
        Audio.PlaySfx(new FakeSfxClip());
        Assert.AreEqual("PlaySfx", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void PlaySfx_WithVolume_RoutesToOverrideOverload()
    {
        Audio.PlaySfx(new FakeSfxClip(), 0.5f);
        Assert.AreEqual("PlaySfxWithVolume", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void Pause_RoutesToService()
    {
        Audio.Pause();
        Assert.AreEqual("Pause", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void Resume_RoutesToService()
    {
        Audio.Resume();
        Assert.AreEqual("Resume", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void StopMusic_RoutesToService()
    {
        Audio.StopMusic();
        Assert.AreEqual("StopMusic", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void StopSfx_RoutesToService()
    {
        Audio.StopSfx();
        Assert.AreEqual("StopSfx", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void StopAll_RoutesToService()
    {
        Audio.StopAll();
        Assert.AreEqual("StopAll", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void MusicStateChanged_SubscriptionRoutesToService()
    {
        AudioStateChangedEventArgs? received = null;
        Audio.MusicStateChanged += args => received = args;

        AudioStateChangedEventArgs expected = new(AudioState.Stopped, AudioState.Playing);
        _fake.RaiseStateChanged(expected);

        Assert.AreSame(expected, received);
    }
}

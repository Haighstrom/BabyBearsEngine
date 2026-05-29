using System;
using System.Collections.Generic;
using System.Linq;
using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Platform.OpenAL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class MusicPlaylistTests
{
    private sealed class FakeMusicClip(string trackName) : IMusicClip
    {
        public string TrackName { get; } = trackName;
        public string Path => $"{TrackName}.wav";
        public TimeSpan Duration => TimeSpan.FromSeconds(10);
        public void Dispose() { }
    }

    private List<IMusicClip> _played = null!;
    private int _stopCount = 0;
    private MusicPlaylist _playlist = null!;
    private FakeMusicClip _trackA = null!;
    private FakeMusicClip _trackB = null!;
    private FakeMusicClip _trackC = null!;

    [TestInitialize]
    public void Setup()
    {
        _played = [];
        _stopCount = 0;
        _trackA = new FakeMusicClip("AAA");
        _trackB = new FakeMusicClip("BBB");
        _trackC = new FakeMusicClip("CCC");
        _playlist = new MusicPlaylist(
            playTrack: clip => _played.Add(clip),
            stopMusic: () => _stopCount++,
            loop: true,
            shuffle: false);
    }

    [TestMethod]
    public void NewPlaylist_HasEmptyState()
    {
        Assert.IsEmpty(_playlist.Tracks);
        Assert.AreEqual(-1, _playlist.CurrentIndex);
        Assert.IsNull(_playlist.CurrentTrack);
    }

    [TestMethod]
    public void Play_OnEmptyPlaylist_IsNoOp()
    {
        _playlist.Play();
        Assert.IsEmpty(_played);
        Assert.AreEqual(-1, _playlist.CurrentIndex);
    }

    [TestMethod]
    public void SetTracks_StoresTracksInOrder_AndResetsIndex()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);

        Assert.HasCount(3, _playlist.Tracks);
        Assert.AreEqual(_trackA, _playlist.Tracks[0]);
        Assert.AreEqual(_trackC, _playlist.Tracks[2]);
        Assert.AreEqual(-1, _playlist.CurrentIndex);
    }

    [TestMethod]
    public void Play_StartsAtFirstTrack_AndInvokesCallback()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play();

        Assert.AreEqual(0, _playlist.CurrentIndex);
        Assert.AreEqual(_trackA, _playlist.CurrentTrack);
        Assert.HasCount(1, _played);
        Assert.AreEqual(_trackA, _played.Single());
    }

    [TestMethod]
    public void NextTrack_AdvancesByOne()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play();
        _playlist.NextTrack();

        Assert.AreEqual(1, _playlist.CurrentIndex);
        Assert.AreEqual(_trackB, _playlist.CurrentTrack);
        CollectionAssert.AreEqual(new[] { _trackA, _trackB }, _played);
    }

    [TestMethod]
    public void NextTrack_WrapsFromEndToStart()
    {
        _playlist.SetTracks(_trackA, _trackB);
        _playlist.Play();
        _playlist.NextTrack();
        _playlist.NextTrack();

        Assert.AreEqual(0, _playlist.CurrentIndex);
        Assert.AreEqual(_trackA, _playlist.CurrentTrack);
    }

    [TestMethod]
    public void PreviousTrack_WrapsFromStartToEnd()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play();
        _playlist.PreviousTrack();

        Assert.AreEqual(2, _playlist.CurrentIndex);
        Assert.AreEqual(_trackC, _playlist.CurrentTrack);
    }

    [TestMethod]
    public void PlayIndex_JumpsToIndex()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play(2);

        Assert.AreEqual(2, _playlist.CurrentIndex);
        Assert.AreEqual(_trackC, _playlist.CurrentTrack);
    }

    [TestMethod]
    public void PlayIndex_WrapsForOutOfRangeIndex()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play(5);

        Assert.AreEqual(2, _playlist.CurrentIndex);
    }

    [TestMethod]
    public void PlayIndex_NegativeIndexWrapsCorrectly()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play(-1);

        Assert.AreEqual(2, _playlist.CurrentIndex);
    }

    [TestMethod]
    public void PlayName_LooksUpTrackByName_CaseInsensitive()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play("bbb");

        Assert.AreEqual(1, _playlist.CurrentIndex);
        Assert.AreEqual(_trackB, _playlist.CurrentTrack);
    }

    [TestMethod]
    public void PlayName_UnknownName_Throws()
    {
        _playlist.SetTracks(_trackA, _trackB);
        Assert.ThrowsExactly<KeyNotFoundException>(() => _playlist.Play("nonexistent"));
    }

    [TestMethod]
    public void PlayClip_JumpsToThatClip()
    {
        _playlist.SetTracks(_trackA, _trackB, _trackC);
        _playlist.Play(_trackC);

        Assert.AreEqual(2, _playlist.CurrentIndex);
        Assert.AreEqual(_trackC, _playlist.CurrentTrack);
    }

    [TestMethod]
    public void PlayClip_ClipNotInPlaylist_Throws()
    {
        _playlist.SetTracks(_trackA, _trackB);
        var foreign = new FakeMusicClip("Foreign");
        Assert.ThrowsExactly<ArgumentException>(() => _playlist.Play(foreign));
    }

    [TestMethod]
    public void OnTrackFinished_AdvancesToNextTrack()
    {
        _playlist.SetTracks(_trackA, _trackB);
        _playlist.Play();
        _playlist.OnTrackFinished();

        Assert.AreEqual(1, _playlist.CurrentIndex);
        CollectionAssert.AreEqual(new[] { _trackA, _trackB }, _played);
    }

    [TestMethod]
    public void OnTrackFinished_AtEndWithLoop_RestartsFromStart()
    {
        _playlist.Loop = true;
        _playlist.SetTracks(_trackA, _trackB);
        _playlist.Play();
        _playlist.OnTrackFinished();
        _playlist.OnTrackFinished();

        Assert.AreEqual(0, _playlist.CurrentIndex);
        Assert.AreEqual(0, _stopCount);
    }

    [TestMethod]
    public void OnTrackFinished_AtEndWithoutLoop_StopsAndClearsIndex()
    {
        _playlist.Loop = false;
        _playlist.SetTracks(_trackA, _trackB);
        _playlist.Play();
        _playlist.OnTrackFinished();
        _playlist.OnTrackFinished();

        Assert.AreEqual(-1, _playlist.CurrentIndex);
        Assert.AreEqual(1, _stopCount);
    }

    [TestMethod]
    public void TrackChanged_FiresWithPreviousAndCurrent()
    {
        var events = new List<MusicTrackChangedEventArgs>();
        _playlist.TrackChanged += events.Add;
        _playlist.SetTracks(_trackA, _trackB);

        _playlist.Play();
        Assert.HasCount(1, events);
        Assert.IsNull(events[0].Previous);
        Assert.AreEqual(_trackA, events[0].Current);

        _playlist.NextTrack();
        Assert.HasCount(2, events);
        Assert.AreEqual(_trackA, events[1].Previous);
        Assert.AreEqual(_trackB, events[1].Current);
    }

    [TestMethod]
    public void Clear_EmptiesTracks_AndStopsMusic()
    {
        _playlist.SetTracks(_trackA, _trackB);
        _playlist.Play();
        _playlist.Clear();

        Assert.IsEmpty(_playlist.Tracks);
        Assert.AreEqual(-1, _playlist.CurrentIndex);
        Assert.AreEqual(1, _stopCount);
    }

    [TestMethod]
    public void Shuffle_ShufflesTracksOnPlay()
    {
        // With 5+ tracks, the chance of a no-op shuffle by accident is 1/120, low enough to ignore.
        _playlist.Shuffle = true;
        var clips = Enumerable.Range(0, 8).Select(i => new FakeMusicClip($"Track{i}")).ToArray();
        var originalOrder = clips.ToArray();
        _playlist.SetTracks(clips);
        _playlist.Play();

        // Tracks property reflects current playback order (which may differ from initial order).
        IReadOnlyList<IMusicClip> shuffled = _playlist.Tracks;
        Assert.HasCount(8, shuffled);
        bool anyDifference = false;
        for (int i = 0; i < shuffled.Count; i++)
        {
            if (!ReferenceEquals(shuffled[i], originalOrder[i]))
            {
                anyDifference = true;
                break;
            }
        }
        Assert.IsTrue(anyDifference, "Expected shuffle to reorder tracks.");
    }
}

using System;
using BabyBearsEngine.AudioSystem;
using BabyBearsEngine.Diagnostics;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Worlds.Graphics.Text;
using BabyBearsEngine.Worlds.UI;
using BabyBearsEngine.Worlds.UI.Themes;

namespace BabyBearsEngine.Demos.Source.Demos.AudioDemo;

/// <summary>
/// Showcases the audio subsystem: master / music / SFX volume sliders, a music playlist with
/// next/previous/pause/resume/stop controls, loop and shuffle toggles, and a pair of buttons to
/// fire one-shot sound effects. The "Now Playing" and "Music State" labels react to the engine's
/// own events so you can see them update when a track finishes naturally too.
/// </summary>
internal class AudioDemoWorld : DemoWorld
{
    private const float ColumnGap = 30f;
    private const float HeaderH = 28f;
    private const float LabelH = 22f;
    private const float SectionW = 360f;
    private const float SliderH = 22f;
    private const float SliderW = 220f;
    private const float StatusH = 30f;
    private const float TopY = 60f;

    private static readonly Colour s_accent = new(70, 130, 200);
    private static readonly FontDefinition s_bodyFont = new("Times New Roman", 14);
    private static readonly FontDefinition s_headerFont = new("Times New Roman", 18);
    private static readonly FontDefinition s_smallFont = new("Times New Roman", 12);

    private IMusicClip? _track1;
    private IMusicClip? _track2;
    private ISfxClip? _gameOverSfx;
    private ISfxClip? _whooshSfx;
    private string? _loadError;

    private TextGraphic _nowPlayingLabel = null!;
    private TextGraphic _stateLabel = null!;
    private TextGraphic _availabilityLabel = null!;
    private Button _pauseResumeButton = null!;

    private TextGraphic _masterValueLabel = null!;
    private TextGraphic _musicValueLabel = null!;
    private TextGraphic _sfxValueLabel = null!;

    public AudioDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(248, 248, 248);

        LoadClips();

        // Back button occupies x=5..85; start the title past its right edge so it stays readable.
        Add(new TextGraphic(s_headerFont, "Audio Demo", Colour.Black, 95, 20, 685, 30)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        BuildVolumeSection(x: 20, y: TopY);
        BuildMusicSection(x: 20 + SectionW + ColumnGap, y: TopY);
        BuildSfxSection(x: 20, y: TopY + 290);
        BuildStatusSection(x: 20 + SectionW + ColumnGap, y: TopY + 360);

        Audio.Playlist.TrackChanged += OnTrackChanged;
        Audio.MusicStateChanged += OnStateChanged;

        RefreshNowPlaying();
        RefreshState();
    }

    public override string Name => "Audio Demo";

    private void LoadClips()
    {
        // Each clip is loaded individually so a single bad asset doesn't take down the whole demo —
        // it shows up in the status label instead. Catches at the call site rather than wrapping
        // the whole method so callers still see which clip failed.
        _track1 = TryLoadMusic("Assets/Audio/Music/birdwatch-season.wav", "Birdwatch Season");
        _track2 = TryLoadMusic("Assets/Audio/Music/birdwatch-season-alt.wav", "Birdwatch Season (alt)");
        _gameOverSfx = TryLoadSfx("Assets/Audio/Sfx/game-over.wav");
        _whooshSfx = TryLoadSfx("Assets/Audio/Sfx/whoosh.wav");

        if (_track1 is not null && _track2 is not null)
        {
            Audio.Playlist.SetTracks(_track1, _track2);
        }
        else if (_track1 is not null)
        {
            Audio.Playlist.SetTracks(_track1);
        }
        else if (_track2 is not null)
        {
            Audio.Playlist.SetTracks(_track2);
        }
    }

    private IMusicClip? TryLoadMusic(string path, string trackName)
    {
        try
        {
            return Audio.LoadMusic(path, trackName);
        }
        catch (Exception ex)
        {
            RecordLoadFailure(path, ex);
            return null;
        }
    }

    private ISfxClip? TryLoadSfx(string path)
    {
        try
        {
            return Audio.LoadSfx(path);
        }
        catch (Exception ex)
        {
            RecordLoadFailure(path, ex);
            return null;
        }
    }

    private void RecordLoadFailure(string path, Exception ex)
    {
        string line = $"Failed to load '{path}': {ex.Message}";
        Logger.Warning("AudioDemo: " + line);
        _loadError = _loadError is null ? line : _loadError + "\n" + line;
    }

    private void BuildVolumeSection(float x, float y)
    {
        Add(MakeHeader(x, y, "Volume"));

        float row = y + HeaderH + 8;

        _masterValueLabel = AddVolumeRow(x, row, "Master", Audio.MasterVolume, v =>
        {
            Audio.MasterVolume = v;
            _masterValueLabel.Text = FormatVolume(v);
        });
        row += SliderH + 18;

        _musicValueLabel = AddVolumeRow(x, row, "Music", Audio.MusicVolume, v =>
        {
            Audio.MusicVolume = v;
            _musicValueLabel.Text = FormatVolume(v);
        });
        row += SliderH + 18;

        _sfxValueLabel = AddVolumeRow(x, row, "SFX", Audio.SfxVolume, v =>
        {
            Audio.SfxVolume = v;
            _sfxValueLabel.Text = FormatVolume(v);
        });
    }

    private TextGraphic AddVolumeRow(float x, float y, string label, float initialValue, Action<float> setter)
    {
        Add(new TextGraphic(s_bodyFont, label, Colour.Black, x, y, 60, SliderH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Scrollbar slider = new(x + 65, y, SliderW, SliderH, ScrollbarDirection.Horizontal,
            ScrollbarTheme.FromColours(new Colour(220, 220, 220), s_accent),
            thumbProportion: 0.08f,
            amountFilled: initialValue);
        slider.ScrollChanged += (_, args) => setter((float)args.NewValue);
        Add(slider);

        TextGraphic valueLabel = new(s_bodyFont, FormatVolume(initialValue), Colour.DimGray,
            x + 65 + SliderW + 10, y, 50, SliderH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(valueLabel);
        return valueLabel;
    }

    private void BuildMusicSection(float x, float y)
    {
        Add(MakeHeader(x, y, "Music"));

        _nowPlayingLabel = new TextGraphic(s_bodyFont, "Now playing: —", Colour.Black,
            x, y + HeaderH + 4, SectionW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_nowPlayingLabel);

        _stateLabel = new TextGraphic(s_smallFont, "State: Stopped", Colour.DimGray,
            x, y + HeaderH + 4 + LabelH, SectionW, LabelH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_stateLabel);

        float buttonY = y + HeaderH + 4 + LabelH * 2 + 8;
        const float btnH = 32f;
        const float btnW = 110f;
        const float btnGap = 6f;

        AddMusicButton(x, buttonY, btnW, btnH, "Play Playlist", () =>
        {
            if (_track1 is not null && _track2 is not null)
            {
                Audio.PlayMusic(_track1, _track2);
            }
            else if (_track1 is not null)
            {
                Audio.PlayMusic(_track1);
            }
            else if (_track2 is not null)
            {
                Audio.PlayMusic(_track2);
            }
        });

        AddMusicButton(x + (btnW + btnGap), buttonY, btnW, btnH, "Stop", () => Audio.StopMusic());

        _pauseResumeButton = MakeButton(x + 2 * (btnW + btnGap), buttonY, btnW, btnH, "Pause", () =>
        {
            if (Audio.MusicState == AudioState.Playing)
            {
                Audio.Pause();
            }
            else if (Audio.MusicState == AudioState.Paused)
            {
                Audio.Resume();
            }
        });
        Add(_pauseResumeButton);

        float row2Y = buttonY + btnH + btnGap;
        AddMusicButton(x, row2Y, btnW, btnH, "<< Previous", () => Audio.Playlist.PreviousTrack());
        AddMusicButton(x + (btnW + btnGap), row2Y, btnW, btnH, "Next >>", () => Audio.Playlist.NextTrack());

        // Direct-jump buttons by name and by index, demonstrating both API styles.
        float row3Y = row2Y + btnH + btnGap;
        AddMusicButton(x, row3Y, btnW, btnH, "Jump: Track 1", () =>
        {
            if (Audio.Playlist.Tracks.Count > 0)
            {
                Audio.Playlist.Play(0);
            }
        });
        AddMusicButton(x + (btnW + btnGap), row3Y, btnW, btnH, "Jump: by name", () =>
        {
            try
            {
                Audio.Playlist.Play("Birdwatch Season (alt)");
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                // Track isn't in the playlist — e.g. someone hit Stop which clears it, or the file
                // failed to load on construction. Re-seed with whatever loaded successfully.
                if (_track1 is not null && _track2 is not null)
                {
                    Audio.Playlist.SetTracks(_track1, _track2);
                    Audio.Playlist.Play("Birdwatch Season (alt)");
                }
            }
        });

        // Loop and shuffle toggles
        float togglesY = row3Y + btnH + 10;
        Checkbox loopBox = new(x, togglesY, 18, 18, CheckboxTheme.Default, isChecked: Audio.Playlist.Loop);
        loopBox.Checked += (_, _) => Audio.Playlist.Loop = true;
        loopBox.Unchecked += (_, _) => Audio.Playlist.Loop = false;
        Add(loopBox);
        Add(new TextGraphic(s_bodyFont, "Loop", Colour.Black, x + 24, togglesY, 60, 18)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });

        Checkbox shuffleBox = new(x + 100, togglesY, 18, 18, CheckboxTheme.Default, isChecked: Audio.Playlist.Shuffle);
        shuffleBox.Checked += (_, _) => Audio.Playlist.Shuffle = true;
        shuffleBox.Unchecked += (_, _) => Audio.Playlist.Shuffle = false;
        Add(shuffleBox);
        Add(new TextGraphic(s_bodyFont, "Shuffle", Colour.Black, x + 124, togglesY, 70, 18)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        });
    }

    private void BuildSfxSection(float x, float y)
    {
        Add(MakeHeader(x, y, "Sound Effects"));

        float buttonY = y + HeaderH + 8;
        const float btnH = 40f;
        const float btnW = 170f;

        Button gameOver = MakeButton(x, buttonY, btnW, btnH, "Play: Game Over", () =>
        {
            if (_gameOverSfx is not null)
            {
                Audio.PlaySfx(_gameOverSfx);
            }
        });
        Add(gameOver);

        Button whoosh = MakeButton(x + btnW + 10, buttonY, btnW, btnH, "Play: Whoosh", () =>
        {
            if (_whooshSfx is not null)
            {
                Audio.PlaySfx(_whooshSfx);
            }
        });
        Add(whoosh);

        // Show off the volume-override overload with a quieter variant.
        Button quietWhoosh = MakeButton(x, buttonY + btnH + 8, btnW, btnH, "Whoosh (50% volume)", () =>
        {
            if (_whooshSfx is not null)
            {
                Audio.PlaySfx(_whooshSfx, 0.5f);
            }
        });
        Add(quietWhoosh);
    }

    private void BuildStatusSection(float x, float y)
    {
        _availabilityLabel = new TextGraphic(s_smallFont,
            Audio.IsAvailable
                ? "Audio subsystem: available"
                : "Audio subsystem: NOT available (see log.log)",
            Audio.IsAvailable ? Colour.DimGray : Colour.Red,
            x, y, SectionW, StatusH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
        Add(_availabilityLabel);

        if (_loadError is not null)
        {
            // Split per-line so TextGraphic (which is single-line) renders each on its own label.
            string[] errorLines = _loadError.Split('\n');
            for (int i = 0; i < errorLines.Length; i++)
            {
                Add(new TextGraphic(s_smallFont, errorLines[i], Colour.Red,
                    x, y + StatusH + i * LabelH, SectionW, LabelH)
                {
                    HAlignment = HAlignment.Left,
                    VAlignment = VAlignment.Centred,
                });
            }
        }
    }

    private void AddMusicButton(float x, float y, float w, float h, string label, Action onClick)
    {
        Add(MakeButton(x, y, w, h, label, onClick));
    }

    private static Button MakeButton(float x, float y, float w, float h, string label, Action onClick)
    {
        Button button = new(x, y, w, h, ButtonTheme.FromColour(s_accent), label);
        button.LeftClicked += (_, _) => onClick();
        return button;
    }

    private static TextGraphic MakeHeader(float x, float y, string text)
    {
        return new TextGraphic(s_headerFont, text, Colour.Black, x, y, SectionW, HeaderH)
        {
            HAlignment = HAlignment.Left,
            VAlignment = VAlignment.Centred,
        };
    }

    private void OnTrackChanged(MusicTrackChangedEventArgs args)
    {
        RefreshNowPlaying();
    }

    private void OnStateChanged(AudioStateChangedEventArgs args)
    {
        RefreshState();
    }

    private void RefreshNowPlaying()
    {
        IMusicClip? current = Audio.Playlist.CurrentTrack;
        _nowPlayingLabel.Text = current is null
            ? "Now playing: —"
            : $"Now playing: {current.TrackName}";
    }

    private void RefreshState()
    {
        AudioState state = Audio.MusicState;
        _stateLabel.Text = $"State: {state}";
        _pauseResumeButton.Text = state == AudioState.Paused ? "Resume" : "Pause";
    }

    private static string FormatVolume(float v) => $"{(int)Math.Round(v * 100)}%";
}

using System;
using System.Collections.Generic;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.Input;
using BabyBearsEngine.Worlds.Collision;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.CollisionDemo;

/// <summary>
/// Showcases overlap detection. An arrow-key-controlled player box turns red while it overlaps
/// any obstacle. A semi-transparent trigger zone — filtered to only react to the player via the
/// Layer/Mask bitmask — prints enter/exit events to the status line. Obstacles ignore one another
/// and ignore the trigger zone.
/// </summary>
internal class CollisionDemoWorld : DemoWorld
{
    private sealed class ArrowKeyMover(IRectAddable target, float speed) : UpdateableBase
    {
        public override void Update(double elapsed)
        {
            float step = speed * (float)elapsed;
            if (Keyboard.KeyDown(Keys.Left))
            {
                target.X -= step;
            }
            if (Keyboard.KeyDown(Keys.Right))
            {
                target.X += step;
            }
            if (Keyboard.KeyDown(Keys.Up))
            {
                target.Y -= step;
            }
            if (Keyboard.KeyDown(Keys.Down))
            {
                target.Y += step;
            }
        }
    }

    // Category bits. Player collides with obstacles + the trigger zone; obstacles + zone only collide with the player.
    private const uint PlayerCategory = 1;
    private const uint ObstacleCategory = 2;
    private const uint ZoneCategory = 4;
    private const float PlayerSpeed = 220f;

    private static readonly Colour s_obstacleColour = new(80, 80, 90);
    private static readonly Colour s_playerColourClear = new(70, 130, 200);
    private static readonly Colour s_playerColourHit = new(220, 70, 70);
    private static readonly Colour s_zoneColourIdle = new(60, 200, 90, 90);
    private static readonly Colour s_zoneColourTriggered = new(60, 200, 90, 180);
    private static readonly FontDefinition s_font = new("Times New Roman", 14);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 16);

    private readonly Dictionary<Collider, string> _colliderLabels = [];
    private readonly HashSet<Collider> _playerOverlaps = [];

    private CollisionSolver _solver = null!;
    private ColourGraphic _playerGraphic = null!;
    private ColourGraphic _zoneGraphic = null!;
    private TextGraphic _statusLabel = null!;
    private TextGraphic _zoneStatusLabel = null!;

    public CollisionDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = new Colour(235, 235, 240);

        // Solver opts into the post-pass via UpdateLast, so add-order with respect to other
        // entities does not matter — it ticks after everything else has moved this frame.
        _solver = new CollisionSolver();
        Add(_solver);

        AddInstructions();
        AddObstacles();
        AddTriggerZone();
        AddPlayer();
        AddStatusLabels();
    }

    public override string Name => "Collision Demo";

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        UpdatePlayerVisuals();
        UpdateZoneVisuals();
        UpdateStatusLabels();
    }

    private void AddInstructions()
    {
        Add(new TextGraphic(s_titleFont, "Collision Demo", Colour.DimGray, 100f, 12f, 600f, 24f)
        {
            HAlignment = HAlignment.Left,
        });
        Add(new TextGraphic(s_font, "Arrow keys to move. Red while overlapping any obstacle. The green zone only reacts to the player.", Colour.DimGray, 100f, 36f, 700f, 20f)
        {
            HAlignment = HAlignment.Left,
        });
    }

    private void AddObstacles()
    {
        (float x, float y, float w, float h, string label)[] obstacles =
        [
            (180f, 140f, 80f, 60f, "Box A"),
            (360f, 220f, 60f, 120f, "Box B"),
            (520f, 160f, 100f, 50f, "Box C"),
            (260f, 380f, 140f, 40f, "Box D"),
        ];

        foreach ((float x, float y, float w, float h, string label) in obstacles)
        {
            Entity obstacle = new(x, y, w, h);
            obstacle.Add(new ColourGraphic(s_obstacleColour, 0f, 0f, w, h, layer: 1));

            Collider collider = new(
                _solver,
                obstacle,
                new RectShape(0f, 0f, w, h),
                collisionCategory: ObstacleCategory,
                collideCategories: PlayerCategory);
            obstacle.Add(collider);
            _colliderLabels[collider] = label;

            Add(new TextGraphic(s_font, label, Colour.White, x, y + h * 0.5f - 9f, w, 18f)
            {
                HAlignment = HAlignment.Centred,
            });

            Add(obstacle);
        }
    }

    private void AddPlayer()
    {
        Entity player = new(60f, 120f, 36f, 36f);
        _playerGraphic = new ColourGraphic(s_playerColourClear, 0f, 0f, 36f, 36f, layer: 1);
        player.Add(_playerGraphic);

        Collider playerCollider = new(
            _solver,
            player,
            new RectShape(0f, 0f, 36f, 36f),
            collisionCategory: PlayerCategory,
            collideCategories: ObstacleCategory | ZoneCategory);
        playerCollider.OverlapEntered += (_, e) => _playerOverlaps.Add(e.Other);
        playerCollider.OverlapExited += (_, e) => _playerOverlaps.Remove(e.Other);
        player.Add(playerCollider);
        _colliderLabels[playerCollider] = "Player";

        // Movement is a child updateable on the player — ticks during the regular pass alongside
        // everything else, then the solver picks up the new position in the post-pass.
        player.Add(new ArrowKeyMover(player, PlayerSpeed));

        Add(player);
    }

    private void AddTriggerZone()
    {
        Entity zone = new(600f, 380f, 160f, 160f);
        _zoneGraphic = new ColourGraphic(s_zoneColourIdle, 0f, 0f, 160f, 160f, layer: 2);
        zone.Add(_zoneGraphic);

        Collider zoneCollider = new(
            _solver,
            zone,
            new RectShape(0f, 0f, 160f, 160f),
            collisionCategory: ZoneCategory,
            collideCategories: PlayerCategory);
        zone.Add(zoneCollider);
        _colliderLabels[zoneCollider] = "Trigger Zone";

        Add(zone);
        Add(new TextGraphic(s_font, "Trigger Zone", Colour.DarkGreen, 600f, 540f, 160f, 18f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void AddStatusLabels()
    {
        _statusLabel = new TextGraphic(s_font, "Status: clear", Colour.DimGray, 100f, 560f, 480f, 20f)
        {
            HAlignment = HAlignment.Left,
        };
        Add(_statusLabel);

        _zoneStatusLabel = new TextGraphic(s_font, "Zone: empty", Colour.DimGray, 100f, 580f, 480f, 20f)
        {
            HAlignment = HAlignment.Left,
        };
        Add(_zoneStatusLabel);
    }

    private void UpdatePlayerVisuals()
    {
        _playerGraphic.Colour = _playerOverlaps.Count > 0 ? s_playerColourHit : s_playerColourClear;
    }

    private void UpdateZoneVisuals()
    {
        bool inZone = false;
        foreach (Collider other in _playerOverlaps)
        {
            if (_colliderLabels.TryGetValue(other, out string? label) && label == "Trigger Zone")
            {
                inZone = true;
                break;
            }
        }
        _zoneGraphic.Colour = inZone ? s_zoneColourTriggered : s_zoneColourIdle;
    }

    private void UpdateStatusLabels()
    {
        if (_playerOverlaps.Count == 0)
        {
            _statusLabel.Text = "Status: clear";
            _zoneStatusLabel.Text = "Zone: empty";
            return;
        }

        List<string> labels = [];
        bool inZone = false;
        foreach (Collider other in _playerOverlaps)
        {
            string label = _colliderLabels.GetValueOrDefault(other, "?");
            labels.Add(label);
            if (label == "Trigger Zone")
            {
                inZone = true;
            }
        }
        labels.Sort();
        _statusLabel.Text = $"Status: overlapping {string.Join(", ", labels)}";
        _zoneStatusLabel.Text = inZone ? "Zone: player inside" : "Zone: empty";
    }
}

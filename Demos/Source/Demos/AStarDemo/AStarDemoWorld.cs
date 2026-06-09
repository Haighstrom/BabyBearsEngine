using System;
using System.Collections.Generic;
using BabyBearsEngine.Input;
using BabyBearsEngine.Pathfinding;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Demos.Source.Demos.AStarDemo;

/// <summary>
/// Visualises A* searching a grid one step at a time. Each tick advances the solver by a single
/// <see cref="AStarSolver{TNode}.Step"/> and recolours the grid from the solver's
/// <see cref="IPathSolver{TNode}.State"/> snapshot, so you can watch the frontier expand and the
/// final path get reconstructed — which is exactly why <c>Step</c> exists separately from
/// <c>TrySolve</c>. Press SPACE to generate a fresh random maze and run again.
/// </summary>
internal sealed class AStarDemoWorld : DemoWorld
{
    private const int GridWidth = 28;
    private const int GridHeight = 18;
    private const float CellSize = 24f;
    private const float CellGap = 1f;
    private const int StartX = 1;
    private const int StartY = 1;
    private const int EndX = GridWidth - 2;
    private const int EndY = GridHeight - 2;
    private const double WallChance = 0.28;
    private const double StepInterval = 0.02;
    private const int MaxStepsPerFrame = 6;

    private static readonly Colour s_background = new(30, 32, 38);
    private static readonly Colour s_empty = new(228, 228, 234);
    private static readonly Colour s_wall = new(48, 52, 62);
    private static readonly Colour s_frontier = new(120, 205, 140);
    private static readonly Colour s_explored = new(245, 200, 130);
    private static readonly Colour s_path = new(70, 130, 235);
    private static readonly Colour s_start = new(40, 175, 80);
    private static readonly Colour s_end = new(220, 70, 70);
    private static readonly Colour s_subtitle = new(190, 192, 200);
    private static readonly FontDefinition s_titleFont = new("Times New Roman", 18);
    private static readonly FontDefinition s_font = new("Times New Roman", 14);

    private readonly float _originX;
    private readonly float _originY;
    private readonly ColourGraphic[,] _cells;
    private readonly TextGraphic _status;
    private readonly Random _random = new();
    private readonly bool[,] _walls = new bool[GridWidth, GridHeight];

    private GridPathfinder<PathfindNode> _grid = null!;
    private AStarSolver<PathfindNode> _solver = null!;
    private IList<PathfindNode>? _path = null;
    private bool _solving = false;
    private double _stepTimer = 0.0;

    public AStarDemoWorld(Func<World> menuWorldFactory) : base(menuWorldFactory)
    {
        BackgroundColour = s_background;

        _originX = (Window.Width - GridWidth * CellSize) / 2f;
        _originY = 92f;

        AddHeader();

        _cells = new ColourGraphic[GridWidth, GridHeight];
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                ColourGraphic cell = new(
                    s_empty,
                    _originX + x * CellSize,
                    _originY + y * CellSize,
                    CellSize - CellGap,
                    CellSize - CellGap,
                    layer: 5);
                _cells[x, y] = cell;
                Add(cell);
            }
        }

        _status = new TextGraphic(s_font, "", Colour.White, 0f, _originY + GridHeight * CellSize + 14f, Window.Width, 20f)
        {
            HAlignment = HAlignment.Centred,
        };
        Add(_status);

        Regenerate();
    }

    public override string Name => "A* Pathfinding Demo";

    public override void Update(double elapsed)
    {
        base.Update(elapsed);

        if (Keyboard.KeyPressed(Keys.Space))
        {
            Regenerate();
            return;
        }

        if (!_solving)
        {
            return;
        }

        _stepTimer += elapsed;

        int stepsThisFrame = 0;
        while (_solving && _stepTimer >= StepInterval && stepsThisFrame < MaxStepsPerFrame)
        {
            _stepTimer -= StepInterval;
            stepsThisFrame++;
            AdvanceOneStep();
        }

        Redraw();
        UpdateStatus();
    }

    private void AddHeader()
    {
        Add(new TextGraphic(s_titleFont, "A* Pathfinding Demo", Colour.White, 100f, 12f, Window.Width - 200f, 24f)
        {
            HAlignment = HAlignment.Centred,
        });
        Add(new TextGraphic(s_font, "Green = frontier, orange = explored, blue = final path.   Press SPACE for a new maze.", s_subtitle, 100f, 44f, Window.Width - 200f, 20f)
        {
            HAlignment = HAlignment.Centred,
        });
    }

    private void AdvanceOneStep()
    {
        SolveStatus status = _solver.Step();

        if (status == SolveStatus.Success)
        {
            _path = _solver.Path;
            _solving = false;
        }
        else if (status == SolveStatus.Failure)
        {
            _path = null;
            _solving = false;
        }
    }

    private void Regenerate()
    {
        GenerateWalls();

        _grid = new GridPathfinder<PathfindNode>(GridWidth, GridHeight, (x, y) => new PathfindNode(x, y));
        PathfindNode start = _grid[StartX, StartY];
        PathfindNode end = _grid[EndX, EndY];

        // Passability is decided per destination cell: an edge is traversable when the node it
        // leads into is not a wall. The solver only ever tests edges out of a node it is already
        // standing on, so checking the destination is sufficient.
        _solver = new AStarSolver<PathfindNode>(start, end, (_, to) => !_walls[(int)to.X, (int)to.Y], Heuristic);

        _path = null;
        _solving = true;
        _stepTimer = 0.0;

        Redraw();
        UpdateStatus();
    }

    private void GenerateWalls()
    {
        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _walls[x, y] = _random.NextDouble() < WallChance;
            }
        }

        // Keep the start, end, and their immediate surroundings clear so a solve is usually
        // possible (and the start/end markers are never hidden under a wall).
        ClearAround(StartX, StartY);
        ClearAround(EndX, EndY);
    }

    private void ClearAround(int centreX, int centreY)
    {
        for (int x = centreX - 1; x <= centreX + 1; x++)
        {
            for (int y = centreY - 1; y <= centreY + 1; y++)
            {
                if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
                {
                    _walls[x, y] = false;
                }
            }
        }
    }

    private void Redraw()
    {
        var (explored, frontier, _) = _solver.State;
        HashSet<PathfindNode> closed = [.. explored];
        HashSet<PathfindNode> open = [.. frontier];
        HashSet<PathfindNode> path = _path is null ? [] : [.. _path];

        for (int x = 0; x < GridWidth; x++)
        {
            for (int y = 0; y < GridHeight; y++)
            {
                _cells[x, y].Colour = ColourFor(x, y, closed, open, path);
            }
        }
    }

    private Colour ColourFor(int x, int y, HashSet<PathfindNode> closed, HashSet<PathfindNode> open, HashSet<PathfindNode> path)
    {
        if (x == StartX && y == StartY)
        {
            return s_start;
        }

        if (x == EndX && y == EndY)
        {
            return s_end;
        }

        if (_walls[x, y])
        {
            return s_wall;
        }

        PathfindNode node = _grid[x, y];

        if (path.Contains(node))
        {
            return s_path;
        }

        if (closed.Contains(node))
        {
            return s_explored;
        }

        if (open.Contains(node))
        {
            return s_frontier;
        }

        return s_empty;
    }

    private void UpdateStatus()
    {
        var (explored, frontier, status) = _solver.State;

        _status.Text = status switch
        {
            SolveStatus.Success => $"Path found — {_path!.Count} cells, {explored.Count} explored.   Press SPACE for a new maze.",
            SolveStatus.Failure => $"No path — the goal is walled off ({explored.Count} explored).   Press SPACE for a new maze.",
            _ => $"Searching…   explored {explored.Count}, frontier {frontier.Count}.",
        };
    }

    private static float Heuristic(PathfindNode a, PathfindNode b) => Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y);
}

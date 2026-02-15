using System;
using System.Collections.Generic;
using System.Text;

namespace BabyBearsEngine.Source.Geometry;

/// <summary>
/// Type that exposes positional X and Y coordinates
/// </summary>
public interface IPosition : IEquatable<IPosition>
{
    /// <summary>
    /// The x-coordinate.
    /// </summary>
    float X { get; }

    /// <summary>
    /// The y-coordinate.
    /// </summary>
    float Y { get; }
}

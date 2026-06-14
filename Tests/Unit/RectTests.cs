using System;
using BabyBearsEngine.Geometry;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class RectTests
{
    private const double Delta = 1e-4;

    // Static factories

    [TestMethod]
    public void EmptyRect_HasZeroPositionAndSize()
    {
        var r = Rect.EmptyRect;
        Assert.AreEqual(0f, r.X);
        Assert.AreEqual(0f, r.Y);
        Assert.AreEqual(0f, r.W);
        Assert.AreEqual(0f, r.H);
    }

    [TestMethod]
    public void UnitRect_IsAtOriginWithSizeOne()
    {
        var r = Rect.UnitRect;
        Assert.AreEqual(0f, r.X);
        Assert.AreEqual(0f, r.Y);
        Assert.AreEqual(1f, r.W);
        Assert.AreEqual(1f, r.H);
    }

    // Constructors

    [TestMethod]
    public void Constructor_XYWH_StoresAllFour()
    {
        Rect r = new(1f, 2f, 3f, 4f);
        Assert.AreEqual(1f, r.X);
        Assert.AreEqual(2f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_Default_IsAllZero()
    {
        Rect r = new();
        Assert.AreEqual(0f, r.X);
        Assert.AreEqual(0f, r.Y);
        Assert.AreEqual(0f, r.W);
        Assert.AreEqual(0f, r.H);
    }

    [TestMethod]
    public void Constructor_WidthHeight_PutsRectAtOrigin()
    {
        Rect r = new(3f, 4f);
        Assert.AreEqual(0f, r.X);
        Assert.AreEqual(0f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_Copy_DuplicatesValues()
    {
        Rect source = new(1f, 2f, 3f, 4f);
        Rect copy = new(source);
        Assert.AreEqual(source.X, copy.X);
        Assert.AreEqual(source.Y, copy.Y);
        Assert.AreEqual(source.W, copy.W);
        Assert.AreEqual(source.H, copy.H);
    }

    [TestMethod]
    public void Constructor_Copy_DoesNotShareState()
    {
        Rect source = new(1f, 2f, 3f, 4f);
        Rect copy = new(source)
        {
            X = 99f
        };
        Assert.AreEqual(1f, source.X);
    }

    [TestMethod]
    public void Constructor_PositionAndExplicitSize_StoresAll()
    {
        Rect r = new(new Point(1f, 2f), 3f, 4f);
        Assert.AreEqual(1f, r.X);
        Assert.AreEqual(2f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_SizeAsPoint_PutsRectAtOrigin()
    {
        Rect r = new(new Point(3f, 4f));
        Assert.AreEqual(0f, r.X);
        Assert.AreEqual(0f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_XYAndSizeAsPoint_StoresAll()
    {
        Rect r = new(1f, 2f, new Point(3f, 4f));
        Assert.AreEqual(1f, r.X);
        Assert.AreEqual(2f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_PositionAndSizeAsPoints_StoresAll()
    {
        Rect r = new(new Point(1f, 2f), new Point(3f, 4f));
        Assert.AreEqual(1f, r.X);
        Assert.AreEqual(2f, r.Y);
        Assert.AreEqual(3f, r.W);
        Assert.AreEqual(4f, r.H);
    }

    [TestMethod]
    public void Constructor_String_ParsesBraceEqualsFormat()
    {
        Rect r = new("{X=1.5,Y=2.5,W=3.5,H=4.5}");
        Assert.AreEqual(1.5f, r.X);
        Assert.AreEqual(2.5f, r.Y);
        Assert.AreEqual(3.5f, r.W);
        Assert.AreEqual(4.5f, r.H);
    }

    [TestMethod]
    public void Constructor_String_ThrowsOnMissingBraces()
    {
        Assert.ThrowsExactly<FormatException>(() => new Rect("X=1,Y=2,W=3,H=4"));
    }

    [TestMethod]
    public void Constructor_String_ThrowsOnWrongPartCount()
    {
        Assert.ThrowsExactly<FormatException>(() => new Rect("{X=1,Y=2,W=3}"));
    }

    [TestMethod]
    public void Constructor_String_ThrowsOnWrongKey()
    {
        Assert.ThrowsExactly<FormatException>(() => new Rect("{X=1,Y=2,Width=3,H=4}"));
    }

    [TestMethod]
    public void Constructor_String_RoundTripsViaToString()
    {
        Rect original = new(1.5f, 2.5f, 3.5f, 4.5f);
        Rect roundTripped = new(original.ToString());
        Assert.AreEqual(original, roundTripped);
    }

    [TestMethod]
    public void Constructor_String_RoundTripsForIntegerValues()
    {
        Rect original = new(1f, 2f, 3f, 4f);
        Rect roundTripped = new(original.ToString());
        Assert.AreEqual(original, roundTripped);
    }

    // Edge accessors

    [TestMethod]
    public void EdgeAccessors_DerivedFromXYWH()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        Assert.AreEqual(10f, r.Left);
        Assert.AreEqual(40f, r.Right);   // X + W
        Assert.AreEqual(20f, r.Top);
        Assert.AreEqual(60f, r.Bottom);  // Y + H
    }

    // Area / Size / SmallestSide / BiggestSide

    [TestMethod]
    public void Area_IsWidthTimesHeight()
    {
        Rect r = new(0f, 0f, 3f, 4f);
        Assert.AreEqual(12f, r.Area);
    }

    [TestMethod]
    public void Size_PacksWAndHIntoPoint()
    {
        Rect r = new(0f, 0f, 3f, 4f);
        Assert.AreEqual(3f, r.Size.X);
        Assert.AreEqual(4f, r.Size.Y);
    }

    [TestMethod]
    public void SmallestSide_ReturnsMinOfWAndH()
    {
        Assert.AreEqual(3f, new Rect(0f, 0f, 5f, 3f).SmallestSide);
        Assert.AreEqual(2f, new Rect(0f, 0f, 2f, 7f).SmallestSide);
    }

    [TestMethod]
    public void BiggestSide_ReturnsMaxOfWAndH()
    {
        Assert.AreEqual(5f, new Rect(0f, 0f, 5f, 3f).BiggestSide);
        Assert.AreEqual(7f, new Rect(0f, 0f, 2f, 7f).BiggestSide);
    }

    // P / corner / centre points

    [TestMethod]
    public void P_GetReturnsTopLeftCorner()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        Assert.AreEqual(10f, r.P.X);
        Assert.AreEqual(20f, r.P.Y);
    }

    [TestMethod]
    public void P_SetUpdatesXAndY()
    {
        Rect r = new(0f, 0f, 30f, 40f);
        r.P = new Point(7f, 8f);
        Assert.AreEqual(7f, r.X);
        Assert.AreEqual(8f, r.Y);
    }

    [TestMethod]
    public void Corners_HaveExpectedPositions()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        Assert.AreEqual(new Point(10f, 20f), r.TopLeft);
        Assert.AreEqual(new Point(40f, 20f), r.TopRight);
        Assert.AreEqual(new Point(10f, 60f), r.BottomLeft);
        Assert.AreEqual(new Point(40f, 60f), r.BottomRight);
    }

    [TestMethod]
    public void EdgeMidpoints_HaveExpectedPositions()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        Assert.AreEqual(new Point(25f, 20f), r.TopCentre);
        Assert.AreEqual(new Point(10f, 40f), r.CentreLeft);
        Assert.AreEqual(new Point(40f, 40f), r.CentreRight);
        Assert.AreEqual(new Point(25f, 60f), r.BottomCentre);
    }

    [TestMethod]
    public void Centre_IsRectMidpoint()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        Assert.AreEqual(new Point(25f, 40f), r.Centre);
    }

    // Zeroed

    [TestMethod]
    public void Zeroed_PreservesSizeAndMovesToOrigin()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var z = r.Zeroed;
        Assert.AreEqual(0f, z.X);
        Assert.AreEqual(0f, z.Y);
        Assert.AreEqual(30f, z.W);
        Assert.AreEqual(40f, z.H);
    }

    [TestMethod]
    public void Zeroed_DoesNotMutateOriginal()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        _ = r.Zeroed;
        Assert.AreEqual(10f, r.X);
    }

    // Resize

    [TestMethod]
    public void Resize_KeepsTopLeft_ChangesSize()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var resized = r.Resize(5f, 6f);
        Assert.AreEqual(10f, resized.X);
        Assert.AreEqual(20f, resized.Y);
        Assert.AreEqual(5f, resized.W);
        Assert.AreEqual(6f, resized.H);
    }

    // Shift

    [TestMethod]
    public void Shift_FloatXY_TranslatesPosition()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var shifted = r.Shift(1f, 2f);
        Assert.AreEqual(11f, shifted.X);
        Assert.AreEqual(22f, shifted.Y);
        Assert.AreEqual(30f, shifted.W);
        Assert.AreEqual(40f, shifted.H);
    }

    [TestMethod]
    public void Shift_WithWHDeltas_AlsoChangesSize()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var shifted = r.Shift(1f, 2f, 3f, 4f);
        Assert.AreEqual(11f, shifted.X);
        Assert.AreEqual(22f, shifted.Y);
        Assert.AreEqual(33f, shifted.W);
        Assert.AreEqual(44f, shifted.H);
    }

    [TestMethod]
    public void Shift_Vector_TranslatesByPointComponents()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var shifted = r.Shift(new Point(1f, 2f));
        Assert.AreEqual(11f, shifted.X);
        Assert.AreEqual(22f, shifted.Y);
    }

    [TestMethod]
    public void Shift_DirectionAndDistance_TranslatesByNormalisedDirection()
    {
        Rect r = new(0f, 0f, 1f, 1f);
        var shifted = r.Shift(new Point(3f, 4f), 10f); // direction (3,4) normalises to (0.6,0.8)
        Assert.AreEqual(6.0, shifted.X, Delta);
        Assert.AreEqual(8.0, shifted.Y, Delta);
    }

    // Scale

    [TestMethod]
    public void Scale_MultipliesSize_LeavesPositionUnchanged()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var scaled = r.Scale(2f, 0.5f);
        Assert.AreEqual(10f, scaled.X);
        Assert.AreEqual(20f, scaled.Y);
        Assert.AreEqual(60f, scaled.W);
        Assert.AreEqual(20f, scaled.H);
    }

    // ScaleAround

    [TestMethod]
    public void ScaleAround_OnTopLeftCorner_BehavesLikeScale()
    {
        // Scaling around a rect's own top-left should match Scale (no position shift).
        Rect r = new(10f, 20f, 30f, 40f);
        var scaled = r.ScaleAround(10f, 20f, 2f, 2f);
        Assert.AreEqual(10f, scaled.X, Delta);
        Assert.AreEqual(20f, scaled.Y, Delta);
        Assert.AreEqual(60f, scaled.W, Delta);
        Assert.AreEqual(80f, scaled.H, Delta);
    }

    [TestMethod]
    public void ScaleAroundCentre_KeepsCentreFixed()
    {
        Rect r = new(0f, 0f, 10f, 10f);
        var scaled = r.ScaleAroundCentre(2f);
        Assert.AreEqual(r.Centre.X, scaled.Centre.X, Delta);
        Assert.AreEqual(r.Centre.Y, scaled.Centre.Y, Delta);
        Assert.AreEqual(20f, scaled.W, Delta);
        Assert.AreEqual(20f, scaled.H, Delta);
    }

    // ResizeAround

    [TestMethod]
    public void ResizeAround_KeepsOriginPointFixed()
    {
        // Resize a 10x10 at (0,0) to 20x20 around its centre (5,5).
        // The new rect should still have the centre as its centre.
        Rect r = new(0f, 0f, 10f, 10f);
        var resized = r.ResizeAround(new Point(5f, 5f), 20f, 20f);
        Assert.AreEqual(5f, resized.Centre.X, Delta);
        Assert.AreEqual(5f, resized.Centre.Y, Delta);
        Assert.AreEqual(20f, resized.W, Delta);
        Assert.AreEqual(20f, resized.H, Delta);
    }

    // Grow

    [TestMethod]
    public void Grow_Uniform_ExpandsAllFourDirections()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var g = r.Grow(5f);
        Assert.AreEqual(5f, g.X);   // X moves left by margin
        Assert.AreEqual(15f, g.Y);  // Y moves up by margin
        Assert.AreEqual(40f, g.W);  // W increases by 2 * margin
        Assert.AreEqual(50f, g.H);
    }

    [TestMethod]
    public void Grow_Negative_Shrinks()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var g = r.Grow(-5f);
        Assert.AreEqual(15f, g.X);
        Assert.AreEqual(25f, g.Y);
        Assert.AreEqual(20f, g.W);
        Assert.AreEqual(30f, g.H);
    }

    [TestMethod]
    public void Grow_PerSide_AppliesEachIndependently()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var g = r.Grow(1f, 2f, 3f, 4f); // left, up, right, down
        Assert.AreEqual(9f, g.X);    // X - 1
        Assert.AreEqual(18f, g.Y);   // Y - 2
        Assert.AreEqual(34f, g.W);   // W + 1 + 3
        Assert.AreEqual(46f, g.H);   // H + 2 + 4
    }

    // Intersects

    [TestMethod]
    public void Intersects_OverlappingRects_ReturnsTrue()
    {
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(5f, 5f, 10f, 10f);
        Assert.IsTrue(a.Intersects(b));
    }

    [TestMethod]
    public void Intersects_DisjointRects_ReturnsFalse()
    {
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(20f, 20f, 5f, 5f);
        Assert.IsFalse(a.Intersects(b));
    }

    [TestMethod]
    public void Intersects_SharedBorder_FalseByDefault_TrueWhenTouchingCounts()
    {
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(10f, 0f, 10f, 10f); // shares right edge of a
        Assert.IsFalse(a.Intersects(b));
        Assert.IsTrue(a.Intersects(b, touchingCounts: true));
    }

    // Intersection

    [TestMethod]
    public void Intersection_OverlappingRects_ReturnsOverlapRegion()
    {
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(5f, 5f, 10f, 10f);
        var i = Rect.Intersection(a, b);
        Assert.AreEqual(5f, i.X);
        Assert.AreEqual(5f, i.Y);
        Assert.AreEqual(5f, i.W);
        Assert.AreEqual(5f, i.H);
    }

    [TestMethod]
    public void Intersection_DisjointRects_ReturnsEmpty()
    {
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(20f, 20f, 5f, 5f);
        var i = Rect.Intersection(a, b);
        Assert.AreEqual(0f, i.W);
        Assert.AreEqual(0f, i.H);
    }

    [TestMethod]
    public void Intersection_EdgeTouchingRects_ReturnsEmpty()
    {
        // Two rects sharing a single edge — Intersects returns false in non-touching mode,
        // so the intersection is empty (matches the prior two-step impl).
        Rect a = new(0f, 0f, 10f, 10f);
        Rect b = new(10f, 0f, 10f, 10f);
        var i = Rect.Intersection(a, b);
        Assert.AreEqual(0f, i.W);
        Assert.AreEqual(0f, i.H);
    }

    [TestMethod]
    public void Intersection_OneInsideOther_ReturnsInner()
    {
        Rect outer = new(0f, 0f, 100f, 100f);
        Rect inner = new(10f, 20f, 30f, 40f);
        var i = Rect.Intersection(outer, inner);
        Assert.AreEqual(10f, i.X);
        Assert.AreEqual(20f, i.Y);
        Assert.AreEqual(30f, i.W);
        Assert.AreEqual(40f, i.H);
    }

    // Contains(Rect)

    [TestMethod]
    public void Contains_Rect_FullyInside_ReturnsTrue()
    {
        Rect outer = new(0f, 0f, 100f, 100f);
        Rect inner = new(10f, 10f, 50f, 50f);
        Assert.IsTrue(outer.Contains(inner));
    }

    [TestMethod]
    public void Contains_Rect_PartiallyOutside_ReturnsFalse()
    {
        Rect outer = new(0f, 0f, 100f, 100f);
        Rect overflow = new(50f, 50f, 60f, 60f); // right edge at 110, outside
        Assert.IsFalse(outer.Contains(overflow));
    }

    [TestMethod]
    public void Contains_Rect_ExactlyOnBorder_ReturnsTrue()
    {
        Rect outer = new(0f, 0f, 10f, 10f);
        Rect onBorder = new(0f, 0f, 10f, 10f);
        Assert.IsTrue(outer.Contains(onBorder));
    }

    // Contains(x, y, w, h)

    [TestMethod]
    public void Contains_XYWH_FullyInside_ReturnsTrue()
    {
        Rect outer = new(0f, 0f, 100f, 100f);
        Assert.IsTrue(outer.Contains(10f, 10f, 50f, 50f));
    }

    [TestMethod]
    public void Contains_XYWH_OverflowingRight_ReturnsFalse()
    {
        // Regression: previously the right-edge check used X + H instead of X + W.
        // For a non-square outer rect (wider than tall), a candidate that fits horizontally
        // but exceeds the height would have been wrongly accepted.
        Rect outer = new(0f, 0f, 100f, 10f);
        Assert.IsFalse(outer.Contains(0f, 0f, 200f, 5f));
    }

    [TestMethod]
    public void Contains_XYWH_OverflowingBottom_ReturnsFalse()
    {
        Rect outer = new(0f, 0f, 100f, 100f);
        Assert.IsFalse(outer.Contains(0f, 0f, 50f, 200f));
    }

    // Contains(Point) edge flags

    [TestMethod]
    public void Contains_Point_DefaultFlags_IncludesLeftAndTopEdges_ExcludesRightAndBottom()
    {
        Rect r = new(0f, 0f, 10f, 10f);
        // Default: onLeftAndTopEdgesCount = true, onRightAndBottomEdgesCount = false
        Assert.IsTrue(r.Contains(new Point(0f, 0f)));    // top-left corner
        Assert.IsTrue(r.Contains(new Point(0f, 5f)));    // on left edge
        Assert.IsFalse(r.Contains(new Point(10f, 5f)));  // on right edge
        Assert.IsFalse(r.Contains(new Point(5f, 10f)));  // on bottom edge
    }

    [TestMethod]
    public void Contains_Point_AllEdgesCount_AcceptsCornerPoints()
    {
        Rect r = new(0f, 0f, 10f, 10f);
        Assert.IsTrue(r.Contains(new Point(10f, 10f), onLeftAndTopEdgesCount: true, onRightAndBottomEdgesCount: true));
    }

    [TestMethod]
    public void Contains_Point_NoEdgesCount_ExcludesAllBorders()
    {
        Rect r = new(0f, 0f, 10f, 10f);
        Assert.IsFalse(r.Contains(new Point(0f, 0f), onLeftAndTopEdgesCount: false, onRightAndBottomEdgesCount: false));
        Assert.IsTrue(r.Contains(new Point(5f, 5f), onLeftAndTopEdgesCount: false, onRightAndBottomEdgesCount: false));
    }

    [TestMethod]
    public void Contains_PointXY_MirrorsContainsPointOverload()
    {
        Rect r = new(0f, 0f, 10f, 10f);
        Assert.IsTrue(r.Contains(0f, 0f));               // default flags include left/top
        Assert.IsFalse(r.Contains(10f, 5f));             // default flags exclude right
        Assert.IsTrue(r.Contains(5f, 5f));               // strictly inside
    }

    // ToVertices

    [TestMethod]
    public void ToVertices_ReturnsClockwiseFromTopLeft()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var v = r.ToVertices();
        Assert.HasCount(4, v);
        Assert.AreEqual(r.TopLeft, v[0]);
        Assert.AreEqual(r.TopRight, v[1]);
        Assert.AreEqual(r.BottomRight, v[2]);
        Assert.AreEqual(r.BottomLeft, v[3]);
    }

    // Equality

    [TestMethod]
    public void Equality_SameValues_AreEqual()
    {
        Rect a = new(1f, 2f, 3f, 4f);
        Rect b = new(1f, 2f, 3f, 4f);
        Assert.IsTrue(a == b);
        Assert.IsTrue(a.Equals(b));
        Assert.AreEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_DifferentValues_AreNotEqual()
    {
        Rect a = new(1f, 2f, 3f, 4f);
        Rect b = new(1f, 2f, 3f, 5f);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equals_NonRect_ReturnsFalse()
    {
        Rect a = new(1f, 2f, 3f, 4f);
        Assert.IsFalse(a.Equals("not a rect"));
        Assert.IsFalse(a.Equals(null));
    }

    [TestMethod]
    public void GetHashCode_DistinguishesRectsWithSmallFractionalDifferences()
    {
        Rect a = new(0f, 0f, 0f, 0.01f);
        Rect b = new(0f, 0f, 0f, 0.02f);
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void GetHashCode_DistinguishesRectsThatDifferOnlyInFractionalPart()
    {
        Rect a = new(1.25f, 2.5f, 3.75f, 4.125f);
        Rect b = new(1.25f, 2.5f, 3.75f, 4.5f);
        Assert.AreNotEqual(a.GetHashCode(), b.GetHashCode());
    }

    [TestMethod]
    public void Equality_NullLeftOperand_DoesNotThrowAndIsNotEqual()
    {
        Rect? a = null;
        Rect b = new(1f, 2f, 3f, 4f);
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equality_NullRightOperand_IsNotEqual()
    {
        Rect a = new(1f, 2f, 3f, 4f);
        Rect? b = null;
        Assert.IsFalse(a == b);
        Assert.IsTrue(a != b);
    }

    [TestMethod]
    public void Equality_BothNull_AreEqual()
    {
        Rect? a = null;
        Rect? b = null;
        Assert.IsTrue(a == b);
        Assert.IsFalse(a != b);
    }

    // Operators

    [TestMethod]
    public void OperatorPlus_RectAndPoint_TranslatesPosition()
    {
        Rect r = new(10f, 20f, 30f, 40f);
        var translated = r + new Point(1f, 2f);
        Assert.AreEqual(11f, translated.X);
        Assert.AreEqual(22f, translated.Y);
        Assert.AreEqual(30f, translated.W);
        Assert.AreEqual(40f, translated.H);
    }

    [TestMethod]
    public void OperatorPlus_TwoRects_AddsComponentwise()
    {
        Rect a = new(1f, 2f, 3f, 4f);
        Rect b = new(10f, 20f, 30f, 40f);
        var sum = a + b;
        Assert.AreEqual(11f, sum.X);
        Assert.AreEqual(22f, sum.Y);
        Assert.AreEqual(33f, sum.W);
        Assert.AreEqual(44f, sum.H);
    }

    [TestMethod]
    public void OperatorMinus_TwoRects_SubtractsComponentwise()
    {
        Rect a = new(11f, 22f, 33f, 44f);
        Rect b = new(1f, 2f, 3f, 4f);
        var diff = a - b;
        Assert.AreEqual(10f, diff.X);
        Assert.AreEqual(20f, diff.Y);
        Assert.AreEqual(30f, diff.W);
        Assert.AreEqual(40f, diff.H);
    }

    // ToString

    [TestMethod]
    public void ToString_ProducesBraceEqualsFormat()
    {
        Rect r = new(1.5f, 2.5f, 3.5f, 4.5f);
        Assert.AreEqual("{X=1.5,Y=2.5,W=3.5,H=4.5}", r.ToString());
    }

    [TestMethod]
    public void ToString_OmitsTrailingZerosForIntegerValues()
    {
        Rect r = new(1f, 2f, 3f, 4f);
        Assert.AreEqual("{X=1,Y=2,W=3,H=4}", r.ToString());
    }
}

namespace BabyBearsEngine.Utilities.Noise;

/// <summary>
/// Per-instance seeded 2D/3D simplex noise. Output range is approximately [-1, 1]. Each instance
/// owns its own permutation table — two instances with the same seed produce identical output and
/// two instances with different seeds produce uncorrelated output. Sampling is thread-safe (no
/// mutable state after construction).
/// </summary>
/// <remarks>
/// Algorithm by Ken Perlin (2001) / Stefan Gustavson (2003-2012). Public-domain reference
/// implementation: <c>https://weber.itn.liu.se/~stegu/simplexnoise/SimplexNoise.java</c>.
/// </remarks>
public sealed class SimplexNoise : INoise2D
{
    // 2D skew/unskew constants. F2 maps the input square grid onto the simplex grid; G2 unmaps.
    private const float F2 = 0.366025403784439f;  // (sqrt(3) - 1) / 2
    private const float G2 = 0.211324865405187f;  // (3 - sqrt(3)) / 6

    // 3D skew/unskew constants.
    private const float F3 = 1f / 3f;
    private const float G3 = 1f / 6f;

    // 12 gradients for 3D simplex; the 2D version uses the (x, y) components of the same set —
    // this is the standard Gustavson trick to avoid maintaining a separate 2D gradient table.
    private static readonly int[][] s_grad3 =
    [
        [ 1,  1, 0], [-1,  1, 0], [ 1, -1, 0], [-1, -1, 0],
        [ 1,  0, 1], [-1,  0, 1], [ 1,  0,-1], [-1,  0,-1],
        [ 0,  1, 1], [ 0, -1, 1], [ 0,  1,-1], [ 0, -1,-1],
    ];

    private readonly int _seed;
    private readonly byte[] _perm;
    private readonly byte[] _permMod12;

    public SimplexNoise()
        : this(Randomisation.Int(int.MaxValue))
    {
    }

    public SimplexNoise(int seed)
    {
        _seed = seed;
        _perm = BuildPermutation(seed);
        _permMod12 = new byte[512];
        for (int i = 0; i < 512; i++)
        {
            _permMod12[i] = (byte)(_perm[i] % 12);
        }
    }

    public int Seed => _seed;

    /// <summary>Returns 2D simplex noise at (<paramref name="x"/>, <paramref name="y"/>). Range ≈ [-1, 1].</summary>
    public float Sample(float x, float y)
    {
        // Skew the input coordinate onto the simplex grid.
        float skew = (x + y) * F2;
        int cellI = FastFloor(x + skew);
        int cellJ = FastFloor(y + skew);

        // Unskew the cell origin back to (x, y) space.
        float unskew = (cellI + cellJ) * G2;
        float originX = cellI - unskew;
        float originY = cellJ - unskew;
        float offset0X = x - originX;
        float offset0Y = y - originY;

        // Determine which simplex (triangle) within the cell we are in.
        int simplexOffsetI;
        int simplexOffsetJ;
        if (offset0X > offset0Y)
        {
            simplexOffsetI = 1;
            simplexOffsetJ = 0;
        }
        else
        {
            simplexOffsetI = 0;
            simplexOffsetJ = 1;
        }

        // Offsets to the other two simplex corners, expressed in (x, y) space.
        float offset1X = offset0X - simplexOffsetI + G2;
        float offset1Y = offset0Y - simplexOffsetJ + G2;
        float offset2X = offset0X - 1f + (2f * G2);
        float offset2Y = offset0Y - 1f + (2f * G2);

        // Wrap cell coords into [0, 255] and look up gradient indices.
        int wrappedI = cellI & 255;
        int wrappedJ = cellJ & 255;
        int gradient0 = _permMod12[wrappedI + _perm[wrappedJ]];
        int gradient1 = _permMod12[wrappedI + simplexOffsetI + _perm[wrappedJ + simplexOffsetJ]];
        int gradient2 = _permMod12[wrappedI + 1 + _perm[wrappedJ + 1]];

        float contribution0 = CornerContribution2D(gradient0, offset0X, offset0Y);
        float contribution1 = CornerContribution2D(gradient1, offset1X, offset1Y);
        float contribution2 = CornerContribution2D(gradient2, offset2X, offset2Y);

        // Scale factor 70 chosen empirically by Gustavson to bring the output to ≈ [-1, 1].
        return 70f * (contribution0 + contribution1 + contribution2);
    }

    /// <summary>Returns 3D simplex noise at (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>). Range ≈ [-1, 1].</summary>
    public float Sample(float x, float y, float z)
    {
        float skew = (x + y + z) * F3;
        int cellI = FastFloor(x + skew);
        int cellJ = FastFloor(y + skew);
        int cellK = FastFloor(z + skew);

        float unskew = (cellI + cellJ + cellK) * G3;
        float originX = cellI - unskew;
        float originY = cellJ - unskew;
        float originZ = cellK - unskew;
        float offset0X = x - originX;
        float offset0Y = y - originY;
        float offset0Z = z - originZ;

        // Identify the simplex (tetrahedron) within the cell by ordering the three offsets.
        int firstStepI;
        int firstStepJ;
        int firstStepK;
        int secondStepI;
        int secondStepJ;
        int secondStepK;
        if (offset0X >= offset0Y)
        {
            if (offset0Y >= offset0Z)
            {
                firstStepI = 1; firstStepJ = 0; firstStepK = 0;
                secondStepI = 1; secondStepJ = 1; secondStepK = 0;
            }
            else if (offset0X >= offset0Z)
            {
                firstStepI = 1; firstStepJ = 0; firstStepK = 0;
                secondStepI = 1; secondStepJ = 0; secondStepK = 1;
            }
            else
            {
                firstStepI = 0; firstStepJ = 0; firstStepK = 1;
                secondStepI = 1; secondStepJ = 0; secondStepK = 1;
            }
        }
        else
        {
            if (offset0Y < offset0Z)
            {
                firstStepI = 0; firstStepJ = 0; firstStepK = 1;
                secondStepI = 0; secondStepJ = 1; secondStepK = 1;
            }
            else if (offset0X < offset0Z)
            {
                firstStepI = 0; firstStepJ = 1; firstStepK = 0;
                secondStepI = 0; secondStepJ = 1; secondStepK = 1;
            }
            else
            {
                firstStepI = 0; firstStepJ = 1; firstStepK = 0;
                secondStepI = 1; secondStepJ = 1; secondStepK = 0;
            }
        }

        float offset1X = offset0X - firstStepI + G3;
        float offset1Y = offset0Y - firstStepJ + G3;
        float offset1Z = offset0Z - firstStepK + G3;
        float offset2X = offset0X - secondStepI + (2f * G3);
        float offset2Y = offset0Y - secondStepJ + (2f * G3);
        float offset2Z = offset0Z - secondStepK + (2f * G3);
        float offset3X = offset0X - 1f + (3f * G3);
        float offset3Y = offset0Y - 1f + (3f * G3);
        float offset3Z = offset0Z - 1f + (3f * G3);

        int wrappedI = cellI & 255;
        int wrappedJ = cellJ & 255;
        int wrappedK = cellK & 255;
        int gradient0 = _permMod12[wrappedI + _perm[wrappedJ + _perm[wrappedK]]];
        int gradient1 = _permMod12[wrappedI + firstStepI + _perm[wrappedJ + firstStepJ + _perm[wrappedK + firstStepK]]];
        int gradient2 = _permMod12[wrappedI + secondStepI + _perm[wrappedJ + secondStepJ + _perm[wrappedK + secondStepK]]];
        int gradient3 = _permMod12[wrappedI + 1 + _perm[wrappedJ + 1 + _perm[wrappedK + 1]]];

        float contribution0 = CornerContribution3D(gradient0, offset0X, offset0Y, offset0Z);
        float contribution1 = CornerContribution3D(gradient1, offset1X, offset1Y, offset1Z);
        float contribution2 = CornerContribution3D(gradient2, offset2X, offset2Y, offset2Z);
        float contribution3 = CornerContribution3D(gradient3, offset3X, offset3Y, offset3Z);

        // Scale factor 32 chosen empirically by Gustavson to bring the output to ≈ [-1, 1].
        return 32f * (contribution0 + contribution1 + contribution2 + contribution3);
    }

    private static byte[] BuildPermutation(int seed)
    {
        byte[] permutation = new byte[256];
        for (int i = 0; i < 256; i++)
        {
            permutation[i] = (byte)i;
        }

        Random random = new(seed);
        for (int i = 255; i > 0; i--)
        {
            int swapIndex = random.Next(i + 1);
            (permutation[i], permutation[swapIndex]) = (permutation[swapIndex], permutation[i]);
        }

        // Duplicate to 512 entries so corner-lookup math can skip a modulo.
        byte[] doubled = new byte[512];
        for (int i = 0; i < 512; i++)
        {
            doubled[i] = permutation[i & 255];
        }
        return doubled;
    }

    private static float CornerContribution2D(int gradientIndex, float offsetX, float offsetY)
    {
        // t is the squared distance from the simplex corner; corners further than r²=0.5 contribute zero.
        float t = 0.5f - (offsetX * offsetX) - (offsetY * offsetY);
        if (t < 0f)
        {
            return 0f;
        }
        t *= t;
        int[] gradient = s_grad3[gradientIndex];
        return t * t * ((gradient[0] * offsetX) + (gradient[1] * offsetY));
    }

    private static float CornerContribution3D(int gradientIndex, float offsetX, float offsetY, float offsetZ)
    {
        float t = 0.6f - (offsetX * offsetX) - (offsetY * offsetY) - (offsetZ * offsetZ);
        if (t < 0f)
        {
            return 0f;
        }
        t *= t;
        int[] gradient = s_grad3[gradientIndex];
        return t * t * ((gradient[0] * offsetX) + (gradient[1] * offsetY) + (gradient[2] * offsetZ));
    }

    private static int FastFloor(float value)
    {
        int truncated = (int)value;
        return value < truncated ? truncated - 1 : truncated;
    }
}

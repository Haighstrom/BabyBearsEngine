namespace BabyBearsEngine.OpenGL;

internal sealed class SpriteTexture(ITexture inner, int columns, int rows, int padding) : ISpriteTexture
{
    private readonly float _paddingU = (float)padding / inner.Width;
    private readonly float _paddingV = (float)padding / inner.Height;

    public int Handle => inner.Handle;

    public int Width => inner.Width;

    public int Height => inner.Height;

    public int Columns { get; } = columns;

    public int Rows { get; } = rows;

    public int Frames => Columns * Rows;

    public float FrameU => (1f - (Columns + 1) * _paddingU) / Columns;

    public float FrameV => (1f - (Rows + 1) * _paddingV) / Rows;

    public (float U1, float V1, float U2, float V2) GetFrameUVs(int frame)
    {
        int col = frame % Columns;
        int row = frame / Columns;
        float u1 = _paddingU + col * (FrameU + _paddingU);
        float v1 = _paddingV + row * (FrameV + _paddingV);
        return (u1, v1, u1 + FrameU, v1 + FrameV);
    }

    public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0)
        => inner.Bind(textureTarget, textureUnit);

    public void Dispose() => inner.Dispose();
}

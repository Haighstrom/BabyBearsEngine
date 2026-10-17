using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Rendering;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Diagnostics;

/// <summary>
/// Diagnostic drawn only while GL tracing is on: a row of small squares in the window's top-left, each rendered with a
/// different shader variant, so a capture shows which techniques work on a given GPU. Uses raw GL apart from the last
/// square, which goes through the engine's own textured path.
/// </summary>
internal static class GLShaderIsolationTest
{
    private const float FirstSquareX = -0.95f;
    private const float SquareGap = 0.15f;
    private const float SquareSize = 0.12f;
    private const float SquareY = 0.8f;

    private const string BlocksFragment = """
        #version 150
        uniform sampler2D Sampler;
        in ColourData { vec4 Colour; } Input_Colour;
        in TexCoordData { vec2 TexCoord; } Input_TexCoord;
        out vec4 Colour;
        void main() { Colour = texture(Sampler, Input_TexCoord.TexCoord) * Input_Colour.Colour; }
        """;

    private const string Blocks150Vertex = """
        #version 150
        uniform vec2 Offset;
        in vec2 Position;
        in vec4 Colour;
        in vec2 TexCoord;
        out ColourData { vec4 Colour; } Output_Colour;
        out TexCoordData { vec2 TexCoord; } Output_TexCoord;
        void main()
        {
            gl_Position = vec4(Position + Offset, 0.0, 1.0);
            Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);
            Output_TexCoord.TexCoord = TexCoord;
        }
        """;

    private const string Blocks330Vertex = """
        #version 330
        uniform vec2 Offset;
        layout (location = 0) in vec2 Position;
        layout (location = 1) in vec4 Colour;
        layout (location = 2) in vec2 TexCoord;
        out ColourData { vec4 Colour; } Output_Colour;
        out TexCoordData { vec2 TexCoord; } Output_TexCoord;
        void main()
        {
            gl_Position = vec4(Position + Offset, 0.0, 1.0);
            Output_Colour.Colour = vec4(Colour.xyz * Colour.w, Colour.w);
            Output_TexCoord.TexCoord = TexCoord;
        }
        """;

    private const string FixedSampleFragment = """
        #version 150
        uniform sampler2D Sampler;
        in vec4 vColour;
        in vec2 vTexCoord;
        out vec4 Colour;
        void main() { Colour = texture(Sampler, vec2(0.25, 0.25)); }
        """;

    private const string Plain150Vertex = """
        #version 150
        uniform vec2 Offset;
        in vec2 Position;
        in vec4 Colour;
        in vec2 TexCoord;
        out vec4 vColour;
        out vec2 vTexCoord;
        void main()
        {
            gl_Position = vec4(Position + Offset, 0.0, 1.0);
            vColour = vec4(Colour.xyz * Colour.w, Colour.w);
            vTexCoord = TexCoord;
        }
        """;

    private const string SolidPlainFragment = """
        #version 150
        in vec4 vColour;
        in vec2 vTexCoord;
        out vec4 Colour;
        void main() { Colour = vec4(1.0, 0.0, 1.0, 1.0); }
        """;

    private const string TexCoordPlainFragment = """
        #version 150
        in vec4 vColour;
        in vec2 vTexCoord;
        out vec4 Colour;
        void main() { Colour = vec4(vTexCoord, 0.0, 1.0); }
        """;

    private const string TexturedPlainFragment = """
        #version 150
        uniform sampler2D Sampler;
        in vec4 vColour;
        in vec2 vTexCoord;
        out vec4 Colour;
        void main() { Colour = texture(Sampler, vTexCoord) * vColour; }
        """;

    private static readonly List<Variant> s_variants = [];
    private static GraphicRenderer? s_engineRenderer;
    private static bool s_initialised = false;
    private static int s_texture = 0;
    private static int s_vao = 0;

    private readonly record struct Variant(int Program, int OffsetLocation, int SamplerLocation);

    /// <summary>Draws the test squares over the finished frame; no-op when GL tracing is off.</summary>
    public static void Draw()
    {
        if (!GLErrorTracer.Enabled)
        {
            return;
        }

        if (!s_initialised)
        {
            Initialise();
        }

        GL.BindVertexArray(s_vao);
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, s_texture);

        for (int index = 0; index < s_variants.Count; index++)
        {
            Variant variant = s_variants[index];
            GL.UseProgram(variant.Program);
            GL.Uniform2(variant.OffsetLocation, FirstSquareX + (index * SquareGap), SquareY);

            if (variant.SamplerLocation >= 0)
            {
                GL.Uniform1(variant.SamplerLocation, 0);
            }

            GL.DrawArrays(PrimitiveType.TriangleStrip, 0, 4);
        }

        // Engine path last, so the engine's own caches are invalidated afterwards.
        OpenGLHelper.ResetBindCache();
        Matrix3 identity = Matrix3.Identity;
        Matrix3 translated = Matrix3.Translate(ref identity, FirstSquareX + (s_variants.Count * SquareGap), SquareY);
        s_engineRenderer!.Render(ref identity, ref translated);
        OpenGLHelper.ResetBindCache();

        GLErrorTracer.Check("shader isolation test");
    }

    private static int CompileProgram(string vertexSource, string fragmentSource)
    {
        int vertex = CompileShader(ShaderType.VertexShader, vertexSource);
        int fragment = CompileShader(ShaderType.FragmentShader, fragmentSource);

        if (vertex == 0 || fragment == 0)
        {
            return 0;
        }

        int program = GL.CreateProgram();
        GL.AttachShader(program, vertex);
        GL.AttachShader(program, fragment);
        GL.BindAttribLocation(program, 0, "Position");
        GL.BindAttribLocation(program, 1, "Colour");
        GL.BindAttribLocation(program, 2, "TexCoord");
        GL.LinkProgram(program);
        GL.GetProgram(program, GetProgramParameterName.LinkStatus, out int linked);

        if (linked == 0)
        {
            Logger.Warning($"Shader isolation test: program failed to link: {GL.GetProgramInfoLog(program).Trim()}");
            return 0;
        }

        return program;
    }

    private static int CompileShader(ShaderType type, string source)
    {
        int shader = GL.CreateShader(type);
        GL.ShaderSource(shader, source);
        GL.CompileShader(shader);
        GL.GetShader(shader, ShaderParameter.CompileStatus, out int compiled);

        if (compiled == 0)
        {
            Logger.Warning($"Shader isolation test: {type} failed to compile: {GL.GetShaderInfoLog(shader).Trim()}");
            return 0;
        }

        return shader;
    }

    private static void AddVariant(string name, string vertexSource, string fragmentSource)
    {
        int program = CompileProgram(vertexSource, fragmentSource);
        Logger.Information($"Shader isolation test square {s_variants.Count + 1}: {name} ({(program == 0 ? "FAILED to build" : "built")}).");

        if (program == 0)
        {
            // Keep the slot so the squares stay in a fixed order; an unbuilt one is skipped by drawing nothing.
            program = CompileProgram(Plain150Vertex, SolidPlainFragment);
        }

        s_variants.Add(new Variant(program, GL.GetUniformLocation(program, "Offset"), GL.GetUniformLocation(program, "Sampler")));
    }

    private static void Initialise()
    {
        s_initialised = true;

        float[] vertices =
        [
            0f, 0f, 1f, 1f, 1f, 1f, 0f, 0f,
            SquareSize, 0f, 1f, 1f, 1f, 1f, 1f, 0f,
            0f, SquareSize, 1f, 1f, 1f, 1f, 0f, 1f,
            SquareSize, SquareSize, 1f, 1f, 1f, 1f, 1f, 1f,
        ];

        int stride = 8 * sizeof(float);
        s_vao = GL.GenVertexArray();
        GL.BindVertexArray(s_vao);
        int buffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, buffer);
        GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, stride, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride, 6 * sizeof(float));

        // 2x2 texture: top-left red, top-right green, bottom-left blue, bottom-right yellow; sampled at (0.25, 0.25) it is red.
        byte[] pixels =
        [
            0, 0, 255, 255, 255, 255, 0, 255,
            255, 0, 0, 255, 0, 255, 0, 255,
        ];

        s_texture = GL.GenTexture();
        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(TextureTarget.Texture2D, s_texture);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
        GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 2, 2, 0, PixelFormat.Rgba, PixelType.UnsignedByte, pixels);

        AddVariant("flat magenta, plain varyings", Plain150Vertex, SolidPlainFragment);
        AddVariant("texture coordinates shown as red/green (gradient), plain varyings", Plain150Vertex, TexCoordPlainFragment);
        AddVariant("texture sampled at a fixed point (should be one solid colour), plain varyings", Plain150Vertex, FixedSampleFragment);
        AddVariant("textured, plain varyings, GLSL 150", Plain150Vertex, TexturedPlainFragment);
        AddVariant("textured, interface blocks, GLSL 150 vertex", Blocks150Vertex, BlocksFragment);
        AddVariant("textured, interface blocks, GLSL 330 vertex (as the engine's own shader)", Blocks330Vertex, BlocksFragment);

        s_engineRenderer = new GraphicRenderer(new Texture(s_texture, 2, 2));
        s_engineRenderer.UpdateVertices(SquareSize, SquareSize, Colour.White);
        Logger.Information($"Shader isolation test square {s_variants.Count + 1}: the engine's own textured path (StandardMatrixShaderProgram).");
        Logger.Information("Shader isolation test: squares run left to right along the window's top-left; the texture is red, green, blue, yellow in quadrants.");

        GLErrorTracer.Check("shader isolation test setup");
    }
}

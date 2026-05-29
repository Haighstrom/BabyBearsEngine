using System;
using BabyBearsEngine.Geometry;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Platform.OpenGL.Shaders.ShaderPrograms;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// Regression coverage for two latent bugs that used to live in <see cref="Sprite.Texture"/>'s
/// setter: (1) it clobbered any custom <see cref="Sprite.Shader"/> by overwriting the renderer's
/// shader with a fresh <see cref="StandardMatrixShaderProgram"/> on every swap (also a GL
/// program leak per swap), and (2) the new texture was only stored on the Sprite — never
/// forwarded to the inner <see cref="Platform.OpenGL.Rendering.GraphicRenderer"/> — so
/// subsequent renders kept binding the original texture. Sprite construction allocates a real
/// shader program, so these run as system tests under a live GL context.
/// </summary>
[TestClass]
public class SpriteTextureSetterTests
{
    private static ApplicationSettings TestSettings => new()
    {
        WindowSettings = new WindowSettings { CheckForMainThread = false, Width = 100, Height = 100 },
        LogSettings = LogSettings.Silent,
    };

    /// <summary>A fake <see cref="ITexture"/> whose <see cref="Bind"/> records call count instead of touching GL state — lets us verify which texture the renderer is actually binding.</summary>
    private sealed class BindCountingTexture(int width, int height) : ITexture
    {
        public int BindCount { get; private set; } = 0;
        public int Handle => 0;
        public int Width { get; } = width;
        public int Height { get; } = height;
        public void Bind(TextureTarget textureTarget = TextureTarget.Texture2D, TextureUnit textureUnit = TextureUnit.Texture0) => BindCount++;
        public void Dispose() { }
    }

    /// <summary>No-op shader for identity-only assertions — never actually used to render.</summary>
    private sealed class StubMatrixShader : IMatrixShaderProgram
    {
        public int Handle => 0;
        public void Bind() { }
        public void SetProjectionMatrix(ref Matrix3 matrix) { }
        public void SetModelViewMatrix(ref Matrix3 matrix) { }
        public void Dispose() { }
    }

    /// <summary>
    /// Minimal harness: runs <paramref name="body"/> once on frame 0 (inside a live GL context),
    /// drains any GL errors the body's manual <see cref="Sprite.Render"/> calls may have left
    /// behind so they don't trip the engine's end-of-frame check, then closes the window.
    /// </summary>
    private sealed class SpriteSetterWorld(Action body) : World
    {
        private int _frame = 0;

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                body();
                while (GL.GetError() != ErrorCode.NoError)
                {
                }
            }
            base.Update(elapsed);
            _frame++;
            if (_frame > 0)
            {
                EngineConfiguration.WindowService.Close();
            }
        }
    }

    [TestMethod]
    public void SettingTextureThenShader_LeavesShaderSet()
    {
        IMatrixShaderProgram customShader = new StubMatrixShader();
        IMatrixShaderProgram? observed = null;
        var world = new SpriteSetterWorld(() =>
        {
            var initialTexture = new SpriteTexture(new BindCountingTexture(64, 64), 1, 1, 0);
            var swappedTexture = new SpriteTexture(new BindCountingTexture(64, 64), 1, 1, 0);
            using var sprite = new Sprite(initialTexture, 0, 0, 32, 32);
            sprite.Texture = swappedTexture;
            sprite.Shader = customShader;
            observed = sprite.Shader;
        });

        GameLauncher.Run(TestSettings, () => world);

        Assert.AreSame(customShader, observed);
    }

    [TestMethod]
    public void SettingShaderThenTexture_LeavesShaderSet()
    {
        IMatrixShaderProgram customShader = new StubMatrixShader();
        IMatrixShaderProgram? observed = null;
        var world = new SpriteSetterWorld(() =>
        {
            var initialTexture = new SpriteTexture(new BindCountingTexture(64, 64), 1, 1, 0);
            var swappedTexture = new SpriteTexture(new BindCountingTexture(64, 64), 1, 1, 0);
            using var sprite = new Sprite(initialTexture, 0, 0, 32, 32);
            sprite.Shader = customShader;
            sprite.Texture = swappedTexture;
            observed = sprite.Shader;
        });

        GameLauncher.Run(TestSettings, () => world);

        Assert.AreSame(customShader, observed);
    }

    [TestMethod]
    public void SettingTexture_CausesSubsequentRendersToBindNewTexture()
    {
        BindCountingTexture initialBindCounter = new(64, 64);
        BindCountingTexture swappedBindCounter = new(64, 64);
        int initialBindsAfterFirstRender = 0;
        int initialBindsAfterSwap = 0;
        int swappedBindsAfterSwap = 0;
        var world = new SpriteSetterWorld(() =>
        {
            using var sprite = new Sprite(new SpriteTexture(initialBindCounter, 1, 1, 0), 0, 0, 32, 32);
            var projection = Matrix3.Identity;
            var modelView = Matrix3.Identity;

            sprite.Render(ref projection, ref modelView);
            initialBindsAfterFirstRender = initialBindCounter.BindCount;

            sprite.Texture = new SpriteTexture(swappedBindCounter, 1, 1, 0);
            sprite.Render(ref projection, ref modelView);

            initialBindsAfterSwap = initialBindCounter.BindCount;
            swappedBindsAfterSwap = swappedBindCounter.BindCount;
        });

        GameLauncher.Run(TestSettings, () => world);

        Assert.IsGreaterThan(0, initialBindsAfterFirstRender, "Initial texture should have been bound by the first Render.");
        Assert.AreEqual(initialBindsAfterFirstRender, initialBindsAfterSwap, "Initial texture must not be bound again after swap.");
        Assert.IsGreaterThan(0, swappedBindsAfterSwap, "Swapped-in texture should have been bound by the post-swap Render.");
    }
}

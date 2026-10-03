using System;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using BabyBearsEngine.Worlds.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.System;

/// <summary>Verifies <see cref="IUpdatableTexture"/> uploads pixels to the GPU, using a live GL context and texture readback.</summary>
[TestClass]
public class UpdatableTextureTests
{
    private static ApplicationSettings TestSettings => new()
    {
        WindowSettings = new WindowSettings { CheckForMainThread = false, Width = 100, Height = 100 },
        LogSettings = LogSettings.Silent,
    };

    private sealed class UpdatableTextureWorld(Action body) : World
    {
        private int _frame = 0;

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                body();
            }

            base.Update(elapsed);
            _frame++;
            EngineConfiguration.WindowService.Close();
        }
    }

    private static byte[] ReadBack(IUpdatableTexture texture)
    {
        byte[] result = new byte[texture.Width * texture.Height * 4];
        texture.Bind();
        GL.GetTexImage(TextureTarget.Texture2D, 0, PixelFormat.Rgba, PixelType.UnsignedByte, result);
        return result;
    }

    [TestMethod]
    public void UpdatePixels_UploadsDataToTexture()
    {
        byte[] expected = [255, 0, 0, 255, 0, 255, 0, 255, 0, 0, 255, 255, 10, 20, 30, 255];
        byte[]? observed = null;
        int width = 0;
        int height = 0;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            using IUpdatableTexture texture = Textures.CreateUpdatable(width: 2, height: 2);
            texture.UpdatePixels(expected);
            width = texture.Width;
            height = texture.Height;
            observed = ReadBack(texture);
        }));

        Assert.AreEqual(2, width);
        Assert.AreEqual(2, height);
        CollectionAssert.AreEqual(expected, observed);
    }

    [TestMethod]
    public void UpdatePixels_CalledTwice_ReplacesPreviousContents()
    {
        byte[] first = [1, 2, 3, 255, 4, 5, 6, 255];
        byte[] second = [200, 100, 50, 255, 9, 8, 7, 255];
        byte[]? observed = null;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            using IUpdatableTexture texture = Textures.CreateUpdatable(width: 2, height: 1);
            texture.UpdatePixels(first);
            texture.UpdatePixels(second);
            observed = ReadBack(texture);
        }));

        CollectionAssert.AreEqual(second, observed);
    }

    [TestMethod]
    public void NewTexture_IsFullyTransparent()
    {
        byte[]? observed = null;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            using IUpdatableTexture texture = Textures.CreateUpdatable(width: 2, height: 2);
            observed = ReadBack(texture);
        }));

        CollectionAssert.AreEqual(new byte[16], observed);
    }

    [TestMethod]
    public void UpdatePixels_WrongLength_Throws()
    {
        bool threw = false;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            using IUpdatableTexture texture = Textures.CreateUpdatable(width: 2, height: 2);
            try
            {
                texture.UpdatePixels(new byte[15]);
            }
            catch (ArgumentException)
            {
                threw = true;
            }
        }));

        Assert.IsTrue(threw);
    }

    [TestMethod]
    public void UpdatePixels_AfterDispose_Throws()
    {
        bool threw = false;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            IUpdatableTexture texture = Textures.CreateUpdatable(width: 1, height: 1);
            texture.Dispose();
            try
            {
                texture.UpdatePixels(new byte[4]);
            }
            catch (ObjectDisposedException)
            {
                threw = true;
            }
        }));

        Assert.IsTrue(threw);
    }

    [TestMethod]
    public void CreateUpdatable_NonPositiveSize_Throws()
    {
        bool zeroWidthThrew = false;
        bool negativeHeightThrew = false;

        GameLauncher.Run(TestSettings, () => new UpdatableTextureWorld(() =>
        {
            try
            {
                Textures.CreateUpdatable(width: 0, height: 4);
            }
            catch (ArgumentOutOfRangeException)
            {
                zeroWidthThrew = true;
            }

            try
            {
                Textures.CreateUpdatable(width: 4, height: -1);
            }
            catch (ArgumentOutOfRangeException)
            {
                negativeHeightThrew = true;
            }
        }));

        Assert.IsTrue(zeroWidthThrew);
        Assert.IsTrue(negativeHeightThrew);
    }
}

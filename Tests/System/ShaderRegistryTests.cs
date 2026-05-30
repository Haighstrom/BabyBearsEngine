using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using BabyBearsEngine.OpenGL;
using BabyBearsEngine.Worlds;
using OpenTK.Graphics.OpenGL4;

namespace BabyBearsEngine.Tests.System;

/// <summary>
/// End-to-end coverage that every shader registered in <see cref="VertexShaders"/>,
/// <see cref="FragmentShaders"/>, <see cref="GeometryShaders"/> (a) points at a file that
/// actually exists on disk, (b) compiles cleanly under a live GL context, and (c) — for every
/// public <see cref="ShaderProgramBase"/> subclass with a parameterless-or-all-defaults
/// constructor — links cleanly and resolves all its uniforms. Catches typoed paths (the
/// <c>grayscale.frag</c> miss from issue-86 follow-up) and GLSL syntax/link errors that
/// otherwise only surface when a game happens to use the shader.
/// </summary>
[TestClass]
public class ShaderRegistryTests
{
    private static ApplicationSettings TestSettings => new()
    {
        WindowSettings = new WindowSettings { CheckForMainThread = false, Width = 100, Height = 100 },
        LogSettings = LogSettings.Silent,
    };

    // -- Path existence ---------------------------------------------------------------------

    [TestMethod]
    public void EveryRegisteredShader_PointsAtFileThatExists()
    {
        var missing = AllShaderEntries()
            .Where(entry => !File.Exists(entry.Path))
            .Select(entry => $"  {entry.RegistryName} → {entry.Path}")
            .ToList();

        Assert.IsEmpty(missing,
            "Registered shader paths that don't resolve to a file on disk:" + Environment.NewLine
            + string.Join(Environment.NewLine, missing));
    }

    // -- Per-shader compile -----------------------------------------------------------------

    private sealed class CompileEveryShaderWorld : World
    {
        private int _frame = 0;

        public List<string> Failures { get; } = [];

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                foreach (ShaderEntry entry in AllShaderEntries())
                {
                    TryCompile(entry);
                }
            }

            _frame++;

            if (_frame > 1)
            {
                EngineConfiguration.WindowService.Close();
            }
        }

        private void TryCompile(ShaderEntry entry)
        {
            int handle = 0;
            try
            {
                string source = File.ReadAllText(entry.Path);
                handle = OpenGLHelper.CreateShader(source, entry.GLShaderType);
            }
            catch (Exception ex)
            {
                // OpenGLHelper.CreateShader throws on a non-zero GL_COMPILE_STATUS with the
                // driver's infoLog appended — collapse to a single line for the assertion message.
                string firstLine = ex.Message.Split('\n', '\r')[0];
                Failures.Add($"  {entry.RegistryName} ({entry.GLShaderType}, {entry.Path}): {firstLine}");
                return;
            }
            finally
            {
                if (handle != 0)
                {
                    GL.DeleteShader(handle);
                }
            }
        }
    }

    [TestMethod]
    public void EveryRegisteredShader_CompilesUnderLiveGLContext()
    {
        var world = new CompileEveryShaderWorld();

        GameLauncher.Run(TestSettings, () => world);

        Assert.IsEmpty(world.Failures,
            "Shaders that failed to compile:" + Environment.NewLine
            + string.Join(Environment.NewLine, world.Failures));
    }

    // -- Per-program construct (catches linking + uniform-location errors) -----------------

    private sealed class ConstructEveryShaderProgramWorld : World
    {
        private int _frame = 0;

        public List<string> Failures { get; } = [];

        public List<string> Constructed { get; } = [];

        public override void Update(double elapsed)
        {
            if (_frame == 0)
            {
                // Drain any GL errors left over from engine boot so subsequent per-program
                // GetError checks attribute errors to the right program.
                DrainGLErrors();

                foreach (Type programType in PublicConstructibleShaderProgramTypes())
                {
                    TryConstruct(programType);
                }

                // Drain again so a final dirty state doesn't poison the next frame's render-loop
                // GL.GetError() check, which would surface as an unhelpful generic
                // "OpenGL error after frame render" rather than the per-program failure above.
                DrainGLErrors();
            }

            _frame++;

            if (_frame > 1)
            {
                EngineConfiguration.WindowService.Close();
            }
        }

        private void TryConstruct(Type programType)
        {
            try
            {
                ConstructorInfo? ctor = AllDefaultsConstructor(programType);
                if (ctor is null)
                {
                    return; // skip — requires explicit args
                }

                object?[] args = ctor.GetParameters().Select(p => p.DefaultValue).ToArray();
                var instance = (IDisposable)ctor.Invoke(args);
                try
                {
                    ErrorCode err = GL.GetError();
                    if (err != ErrorCode.NoError)
                    {
                        Failures.Add($"  {programType.FullName}: GL error after construct = {err}");
                    }
                    else
                    {
                        Constructed.Add(programType.Name);
                    }
                }
                finally
                {
                    instance.Dispose();
                    DrainGLErrors();
                }
            }
            catch (Exception ex)
            {
                // Unwrap reflection's TargetInvocationException so the real engine error surfaces.
                Exception real = ex is TargetInvocationException tie && tie.InnerException is not null
                    ? tie.InnerException
                    : ex;
                string firstLine = real.Message.Split('\n', '\r')[0];
                Failures.Add($"  {programType.FullName}: {real.GetType().Name}: {firstLine}");
                DrainGLErrors();
            }
        }

        private static void DrainGLErrors()
        {
            while (GL.GetError() != ErrorCode.NoError)
            {
                // discard
            }
        }
    }

    [TestMethod]
    public void EveryPublicShaderProgram_ConstructsWithoutError()
    {
        var world = new ConstructEveryShaderProgramWorld();

        GameLauncher.Run(TestSettings, () => world);

        Assert.IsNotEmpty(world.Constructed,
            "Reflection found no constructible shader programs — registry walk is broken.");
        Assert.IsEmpty(world.Failures,
            "Shader programs that failed to construct:" + Environment.NewLine
            + string.Join(Environment.NewLine, world.Failures));
    }

    // -- Reflection helpers -----------------------------------------------------------------

    private readonly record struct ShaderEntry(string RegistryName, string Path, ShaderType GLShaderType);

    private static IEnumerable<ShaderEntry> AllShaderEntries()
    {
        foreach (ShaderEntry entry in EntriesFrom(typeof(VertexShaders), typeof(VertexShaderPath), ShaderType.VertexShader))
        {
            yield return entry;
        }
        foreach (ShaderEntry entry in EntriesFrom(typeof(FragmentShaders), typeof(FragmentShaderPath), ShaderType.FragmentShader))
        {
            yield return entry;
        }
        foreach (ShaderEntry entry in EntriesFrom(typeof(GeometryShaders), typeof(GeometryShaderPath), ShaderType.GeometryShader))
        {
            yield return entry;
        }
    }

    private static IEnumerable<ShaderEntry> EntriesFrom(Type registryType, Type pathRecordType, ShaderType shaderType)
    {
        PropertyInfo pathProperty = pathRecordType.GetProperty("Path")
            ?? throw new InvalidOperationException($"{pathRecordType.Name} has no Path property.");

        foreach (PropertyInfo property in registryType.GetProperties(BindingFlags.Public | BindingFlags.Static))
        {
            if (property.PropertyType != pathRecordType)
            {
                continue;
            }

            object? value = property.GetValue(null);
            if (value is null)
            {
                continue;
            }

            string path = (string)pathProperty.GetValue(value)!;
            yield return new ShaderEntry(property.Name, path, shaderType);
        }
    }

    /// <summary>
    /// Walks BBE's assembly for public, non-abstract <see cref="ShaderProgramBase"/> subclasses.
    /// Singleton shader programs (which expose a static <c>Instance</c>) are excluded — direct
    /// construction would bypass their cache and risk leaking GL resources across test runs.
    /// </summary>
    private static IEnumerable<Type> PublicConstructibleShaderProgramTypes()
    {
        Assembly bbe = typeof(ShaderProgramBase).Assembly;
        foreach (Type type in bbe.GetTypes())
        {
            if (!type.IsPublic || type.IsAbstract)
            {
                continue;
            }
            if (!typeof(ShaderProgramBase).IsAssignableFrom(type))
            {
                continue;
            }
            if (type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static) is not null)
            {
                continue;
            }
            yield return type;
        }
    }

    private static ConstructorInfo? AllDefaultsConstructor(Type type)
    {
        return type.GetConstructors()
            .Where(ctor => ctor.GetParameters().All(p => p.HasDefaultValue))
            .OrderBy(ctor => ctor.GetParameters().Length)
            .FirstOrDefault();
    }
}

using System;
using BabyBearsEngine.OpenGL;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class GpuCapabilitiesTests
{
    [TestCleanup]
    public void Cleanup() => GpuCapabilities.Reset();

    // -- Current / IsAvailable ---------------------------------------------------------------

    [TestMethod]
    public void Current_BeforePopulate_Throws()
    {
        Assert.ThrowsExactly<InvalidOperationException>(() => _ = GpuCapabilities.Current);
    }

    [TestMethod]
    public void IsAvailable_BeforePopulate_IsFalse()
    {
        Assert.IsFalse(GpuCapabilities.IsAvailable);
    }

    [TestMethod]
    public void Current_AfterSetForTesting_ReturnsStoredInstance()
    {
        GpuCapabilities expected = new(new Version(4, 5), glslVersion: 450, maxMsaaSamples: 8);

        GpuCapabilities.SetForTesting(expected);

        Assert.AreSame(expected, GpuCapabilities.Current);
        Assert.IsTrue(GpuCapabilities.IsAvailable);
    }

    [TestMethod]
    public void Reset_ClearsCurrent()
    {
        GpuCapabilities.SetForTesting(new GpuCapabilities(new Version(4, 5), 450, 8));

        GpuCapabilities.Reset();

        Assert.IsFalse(GpuCapabilities.IsAvailable);
    }

    // -- OpenGLToGlslVersion ----------------------------------------------------------------

    [TestMethod]
    public void OpenGLToGlslVersion_GL45_Returns450()
    {
        Assert.AreEqual(450, GpuCapabilities.OpenGLToGlslVersion(4, 5));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL43_Returns430()
    {
        Assert.AreEqual(430, GpuCapabilities.OpenGLToGlslVersion(4, 3));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL40_Returns400()
    {
        Assert.AreEqual(400, GpuCapabilities.OpenGLToGlslVersion(4, 0));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL33_Returns330()
    {
        Assert.AreEqual(330, GpuCapabilities.OpenGLToGlslVersion(3, 3));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL32_Returns150()
    {
        Assert.AreEqual(150, GpuCapabilities.OpenGLToGlslVersion(3, 2));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL31_Returns140()
    {
        Assert.AreEqual(140, GpuCapabilities.OpenGLToGlslVersion(3, 1));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL30_Returns130()
    {
        Assert.AreEqual(130, GpuCapabilities.OpenGLToGlslVersion(3, 0));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL21_Returns120()
    {
        Assert.AreEqual(120, GpuCapabilities.OpenGLToGlslVersion(2, 1));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_GL20_Returns110()
    {
        Assert.AreEqual(110, GpuCapabilities.OpenGLToGlslVersion(2, 0));
    }

    [TestMethod]
    public void OpenGLToGlslVersion_Pre20_ReturnsZero()
    {
        Assert.AreEqual(0, GpuCapabilities.OpenGLToGlslVersion(1, 5));
    }

    // -- TryParseGlslVersion ----------------------------------------------------------------

    [TestMethod]
    public void TryParseGlslVersion_SimpleVersion_Returns()
    {
        Assert.AreEqual(330, GpuCapabilities.TryParseGlslVersion("#version 330\nin vec2 Position;"));
    }

    [TestMethod]
    public void TryParseGlslVersion_WithCoreProfile_Returns()
    {
        Assert.AreEqual(330, GpuCapabilities.TryParseGlslVersion("#version 330 core\nin vec2 Position;"));
    }

    [TestMethod]
    public void TryParseGlslVersion_WithCompatibilityProfile_Returns()
    {
        Assert.AreEqual(330, GpuCapabilities.TryParseGlslVersion("#version 330 compatibility"));
    }

    [TestMethod]
    public void TryParseGlslVersion_LeadingLineComment_Returns()
    {
        const string source = "// Vertex shader for sprite rendering\n#version 150\nin vec2 Position;";
        Assert.AreEqual(150, GpuCapabilities.TryParseGlslVersion(source));
    }

    [TestMethod]
    public void TryParseGlslVersion_LeadingBlankLines_Returns()
    {
        const string source = "\n\n#version 430\nin vec2 Position;";
        Assert.AreEqual(430, GpuCapabilities.TryParseGlslVersion(source));
    }

    [TestMethod]
    public void TryParseGlslVersion_LeadingCommentAndBlankLines_Returns()
    {
        const string source = "// header\n\n   // second comment\n#version 130\nout vec4 color;";
        Assert.AreEqual(130, GpuCapabilities.TryParseGlslVersion(source));
    }

    [TestMethod]
    public void TryParseGlslVersion_NoVersionDirective_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.TryParseGlslVersion("in vec2 Position;\n#version 330"));
    }

    [TestMethod]
    public void TryParseGlslVersion_EmptySource_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.TryParseGlslVersion(""));
    }

    [TestMethod]
    public void TryParseGlslVersion_VersionMissingNumber_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.TryParseGlslVersion("#version\nin vec2 Position;"));
    }

    [TestMethod]
    public void TryParseGlslVersion_VersionNonNumericArg_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.TryParseGlslVersion("#version foo\nin vec2 Position;"));
    }

    // -- EnforceEngineMinimum ---------------------------------------------------------------

    [TestMethod]
    public void EnforceEngineMinimum_ReportedExceedsMinimum_DoesNotThrow()
    {
        GpuCapabilities.EnforceEngineMinimum(new Version(4, 5), new Version(3, 2));
    }

    [TestMethod]
    public void EnforceEngineMinimum_ReportedEqualsMinimum_DoesNotThrow()
    {
        GpuCapabilities.EnforceEngineMinimum(new Version(3, 2), new Version(3, 2));
    }

    [TestMethod]
    public void EnforceEngineMinimum_ReportedBelowMinimumByMinor_Throws()
    {
        Assert.ThrowsExactly<EngineInitialisationException>(
            () => GpuCapabilities.EnforceEngineMinimum(new Version(3, 1), new Version(3, 2)));
    }

    [TestMethod]
    public void EnforceEngineMinimum_ReportedBelowMinimumByMajor_Throws()
    {
        Assert.ThrowsExactly<EngineInitialisationException>(
            () => GpuCapabilities.EnforceEngineMinimum(new Version(2, 1), new Version(3, 2)));
    }

    [TestMethod]
    public void EnforceEngineMinimum_ExceptionMessageNamesBothVersions()
    {
        var ex = Assert.ThrowsExactly<EngineInitialisationException>(
            () => GpuCapabilities.EnforceEngineMinimum(new Version(3, 1), new Version(3, 2)));

        Assert.Contains("3.1", ex.Message);
        Assert.Contains("3.2", ex.Message);
    }

    // -- EnforceShaderRequirement -----------------------------------------------------------

    [TestMethod]
    public void EnforceShaderRequirement_RequiredBelowAvailable_DoesNotThrow()
    {
        GpuCapabilities.EnforceShaderRequirement("foo.frag", shaderGlslVersion: 150, gpuGlslVersion: 330);
    }

    [TestMethod]
    public void EnforceShaderRequirement_RequiredEqualsAvailable_DoesNotThrow()
    {
        GpuCapabilities.EnforceShaderRequirement("foo.frag", shaderGlslVersion: 330, gpuGlslVersion: 330);
    }

    [TestMethod]
    public void EnforceShaderRequirement_RequiredAboveAvailable_Throws()
    {
        var ex = Assert.ThrowsExactly<ShaderRequiresGLVersionException>(
            () => GpuCapabilities.EnforceShaderRequirement("lighting.frag", 400, 330));

        Assert.AreEqual("lighting.frag", ex.ShaderIdentifier);
        Assert.AreEqual(400, ex.RequiredGlslVersion);
        Assert.AreEqual(330, ex.AvailableGlslVersion);
    }

    [TestMethod]
    public void EnforceShaderRequirement_ExceptionMessageNamesShaderAndVersions()
    {
        var ex = Assert.ThrowsExactly<ShaderRequiresGLVersionException>(
            () => GpuCapabilities.EnforceShaderRequirement("lighting.frag", 400, 330));

        Assert.Contains("lighting.frag", ex.Message);
        Assert.Contains("400", ex.Message);
        Assert.Contains("330", ex.Message);
    }

    // -- GetGrantedBelowRequestedWarning ----------------------------------------------------

    [TestMethod]
    public void GetGrantedBelowRequestedWarning_GrantedExceedsRequested_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.GetGrantedBelowRequestedWarning(
            requested: new Version(4, 3), granted: new Version(4, 5)));
    }

    [TestMethod]
    public void GetGrantedBelowRequestedWarning_GrantedEqualsRequested_ReturnsNull()
    {
        Assert.IsNull(GpuCapabilities.GetGrantedBelowRequestedWarning(
            requested: new Version(4, 5), granted: new Version(4, 5)));
    }

    [TestMethod]
    public void GetGrantedBelowRequestedWarning_GrantedBelowRequestedByMinor_ReturnsWarning()
    {
        string? warning = GpuCapabilities.GetGrantedBelowRequestedWarning(
            requested: new Version(4, 5), granted: new Version(4, 3));

        Assert.IsNotNull(warning);
        Assert.Contains("4.5", warning);
        Assert.Contains("4.3", warning);
    }

    [TestMethod]
    public void GetGrantedBelowRequestedWarning_GrantedBelowRequestedByMajor_ReturnsWarning()
    {
        string? warning = GpuCapabilities.GetGrantedBelowRequestedWarning(
            requested: new Version(4, 5), granted: new Version(3, 3));

        Assert.IsNotNull(warning);
        Assert.Contains("4.5", warning);
        Assert.Contains("3.3", warning);
    }

    // -- EnforceShaderRequirement (source-overload) ----------------------------------------

    [TestMethod]
    public void EnforceShaderRequirementFromSource_NoCapabilities_DoesNotThrow()
    {
        GpuCapabilities.EnforceShaderRequirement("foo.frag", "#version 999\nout vec4 c;");
    }

    [TestMethod]
    public void EnforceShaderRequirementFromSource_RequiredAboveAvailable_Throws()
    {
        GpuCapabilities.SetForTesting(new GpuCapabilities(new Version(3, 3), 330, 8));

        var ex = Assert.ThrowsExactly<ShaderRequiresGLVersionException>(
            () => GpuCapabilities.EnforceShaderRequirement("lighting.frag", "#version 400\nout vec4 c;"));

        Assert.AreEqual("lighting.frag", ex.ShaderIdentifier);
        Assert.AreEqual(400, ex.RequiredGlslVersion);
        Assert.AreEqual(330, ex.AvailableGlslVersion);
    }

    [TestMethod]
    public void EnforceShaderRequirementFromSource_RequiredBelowAvailable_DoesNotThrow()
    {
        GpuCapabilities.SetForTesting(new GpuCapabilities(new Version(4, 5), 450, 8));

        GpuCapabilities.EnforceShaderRequirement("foo.frag", "#version 150\nout vec4 c;");
    }

    [TestMethod]
    public void EnforceShaderRequirementFromSource_NoVersionDirective_DoesNotThrow()
    {
        GpuCapabilities.SetForTesting(new GpuCapabilities(new Version(3, 2), 150, 4));

        GpuCapabilities.EnforceShaderRequirement("foo.frag", "out vec4 c;");
    }
}

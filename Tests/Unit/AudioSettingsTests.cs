namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class AudioSettingsTests
{
    [TestMethod]
    public void Default_HasAudioEnabled_AndFullVolumes()
    {
        AudioSettings settings = AudioSettings.Default;

        Assert.IsFalse(settings.Disabled);
        Assert.AreEqual(1f, settings.MasterVolume);
        Assert.AreEqual(1f, settings.MusicVolume);
        Assert.AreEqual(1f, settings.SfxVolume);
        Assert.IsGreaterThan(0, settings.MaxSfxChannels);
        Assert.IsTrue(settings.LoopMusic);
    }

    [TestMethod]
    public void Silent_DisablesAudio()
    {
        AudioSettings settings = AudioSettings.Silent;
        Assert.IsTrue(settings.Disabled);
    }

    [TestMethod]
    public void InitOnly_AllowsOverridingSpecificFields()
    {
        AudioSettings settings = AudioSettings.Default with { MasterVolume = 0.5f, MaxSfxChannels = 32 };

        Assert.AreEqual(0.5f, settings.MasterVolume);
        Assert.AreEqual(32, settings.MaxSfxChannels);
        Assert.AreEqual(1f, settings.MusicVolume); // unchanged
    }
}

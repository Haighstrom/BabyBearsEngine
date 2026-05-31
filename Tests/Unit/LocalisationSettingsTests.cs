namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class LocalisationSettingsTests
{
    [TestMethod]
    public void Default_HasExpectedAssetsFolderAndLocales()
    {
        LocalisationSettings settings = LocalisationSettings.Default;

        Assert.AreEqual("Assets/Localisation/", settings.AssetsFolder);
        Assert.AreEqual("en", settings.DefaultLocale);
        Assert.AreEqual("en", settings.FallbackLocale);
    }

    [TestMethod]
    public void InitOnly_AllowsOverridingSpecificFields()
    {
        LocalisationSettings settings = LocalisationSettings.Default with
        {
            DefaultLocale = "fr",
            FallbackLocale = "en",
        };

        Assert.AreEqual("fr", settings.DefaultLocale);
        Assert.AreEqual("en", settings.FallbackLocale);
        Assert.AreEqual("Assets/Localisation/", settings.AssetsFolder); // unchanged
    }
}

using System;
using System.Collections.Generic;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class StringsFacadeTests
{
    private sealed class FakeLocaliser : ILocaliser
    {
        public List<(string Method, object? Arg)> Calls { get; } = [];

        public string CurrentLocale { get; set; } = "en";
        public IReadOnlyList<string> AvailableLocales { get; set; } = ["en", "fr"];

        public string Get(string key)
        {
            Calls.Add(("Get", key));
            return $"translated:{key}";
        }

        public string Format(string key, params object[] args)
        {
            Calls.Add(("Format", (key, args)));
            return $"formatted:{key}:{args.Length}";
        }

        public void SetLocale(string locale)
        {
            Calls.Add(("SetLocale", locale));
            CurrentLocale = locale;
        }

        public event Action<LocaleChangedEventArgs>? LocaleChanged;

        public void RaiseLocaleChanged(LocaleChangedEventArgs args) => LocaleChanged?.Invoke(args);
    }

    private FakeLocaliser _fake = null!;

    [TestInitialize]
    public void Setup()
    {
        _fake = new FakeLocaliser();
        EngineConfiguration.LocalisationService = _fake;
    }

    [TestCleanup]
    public void Cleanup() => EngineConfiguration.Reset();

    [TestMethod]
    public void CurrentLocale_ProxiesService()
    {
        _fake.CurrentLocale = "fr";
        Assert.AreEqual("fr", Strings.CurrentLocale);
    }

    [TestMethod]
    public void AvailableLocales_ProxiesService()
    {
        _fake.AvailableLocales = ["en", "fr", "ja"];
        Assert.AreSame(_fake.AvailableLocales, Strings.AvailableLocales);
    }

    [TestMethod]
    public void Get_RoutesToService()
    {
        string result = Strings.Get("menu.play");

        Assert.AreEqual("translated:menu.play", result);
        Assert.AreEqual(("Get", (object?)"menu.play"), _fake.Calls.Single());
    }

    [TestMethod]
    public void Format_RoutesToService()
    {
        string result = Strings.Format("shop.coins", 7);

        Assert.AreEqual("formatted:shop.coins:1", result);
        Assert.AreEqual("Format", _fake.Calls.Single().Method);
    }

    [TestMethod]
    public void SetLocale_RoutesToService()
    {
        Strings.SetLocale("fr");

        Assert.AreEqual(("SetLocale", (object?)"fr"), _fake.Calls.Single());
    }

    [TestMethod]
    public void LocaleChanged_SubscriptionRoutesToService()
    {
        LocaleChangedEventArgs? received = null;
        Strings.LocaleChanged += args => received = args;

        LocaleChangedEventArgs expected = new("en", "fr");
        _fake.RaiseLocaleChanged(expected);

        Assert.AreSame(expected, received);
    }

    [TestMethod]
    public void LocalisationService_DefaultsToNullLocaliser()
    {
        EngineConfiguration.Reset();

        Assert.IsInstanceOfType<NullLocaliser>(EngineConfiguration.LocalisationService);
    }
}

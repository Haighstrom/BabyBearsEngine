using System;
using System.Collections.Generic;
using System.IO;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tests.Unit;

[TestClass]
public class FontImporterTests
{
    private string _tempRoot = "";

    [TestInitialize]
    public void Setup()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), "BBE_FontImporterTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempRoot);
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_tempRoot))
        {
            Directory.Delete(_tempRoot, recursive: true);
        }
    }

    private string MakeSourceFolder(params string[] fileNames)
    {
        string folder = Path.Combine(_tempRoot, "src");
        Directory.CreateDirectory(folder);

        foreach (string fileName in fileNames)
        {
            // Empty body — the importer only reads filenames, not font contents.
            File.WriteAllBytes(Path.Combine(folder, fileName), []);
        }

        return folder;
    }

    private string DestFolder() => Path.Combine(_tempRoot, "dst");

    // ── ParseFilename: canonical patterns ──────────────────────────────────

    [TestMethod]
    public void ParseFilename_RegularSuffix_ClassifiesAsRegular()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.Regular),
            FontImporter.ParseFilename("Roboto-Regular"));
    }

    [TestMethod]
    public void ParseFilename_BoldSuffix_ClassifiesAsBold()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.Bold),
            FontImporter.ParseFilename("Roboto-Bold"));
    }

    [TestMethod]
    public void ParseFilename_ItalicSuffix_ClassifiesAsItalic()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.Italic),
            FontImporter.ParseFilename("Roboto-Italic"));
    }

    [TestMethod]
    public void ParseFilename_ObliqueSuffix_ClassifiesAsItalic()
    {
        Assert.AreEqual(
            ("Lobster", FontImporter.FontVariantClassification.Italic),
            FontImporter.ParseFilename("Lobster-Oblique"));
    }

    [TestMethod]
    public void ParseFilename_BoldItalicCompoundNoSeparator_ClassifiesAsBoldItalic()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("Roboto-BoldItalic"));
    }

    [TestMethod]
    public void ParseFilename_BoldItalicWithSpaceInside_ClassifiesAsBoldItalic()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("Roboto-Bold Italic"));
    }

    [TestMethod]
    public void ParseFilename_BoldItalicWithDashInside_ClassifiesAsBoldItalic()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("Roboto-Bold-Italic"));
    }

    [TestMethod]
    public void ParseFilename_ItalicBoldOrder_ClassifiesAsBoldItalic()
    {
        // Rare order, but some packs use it.
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("Roboto-Italic-Bold"));
    }

    [TestMethod]
    public void ParseFilename_BoldObliqueCompound_ClassifiesAsBoldItalic()
    {
        Assert.AreEqual(
            ("Lobster", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("Lobster-BoldOblique"));
    }

    // ── ParseFilename: separator variants ──────────────────────────────────

    [TestMethod]
    public void ParseFilename_SpaceSeparator_ClassifiesCorrectly()
    {
        Assert.AreEqual(
            ("Lato", FontImporter.FontVariantClassification.Bold),
            FontImporter.ParseFilename("Lato Bold"));
    }

    [TestMethod]
    public void ParseFilename_UnderscoreSeparator_ClassifiesCorrectly()
    {
        Assert.AreEqual(
            ("Lato", FontImporter.FontVariantClassification.Bold),
            FontImporter.ParseFilename("Lato_Bold"));
    }

    // ── ParseFilename: CamelCase (smushed) suffix ─────────────────────────

    [TestMethod]
    public void ParseFilename_CamelCaseItalicSuffix_RecognisedAsItalic()
    {
        // Zodiak ships its italic as "Zodiak-VariableItalic.ttf" — the "Italic" is glued onto
        // "Variable" with no separator. Must still classify as Italic.
        Assert.AreEqual(
            ("Zodiak-Variable", FontImporter.FontVariantClassification.Italic),
            FontImporter.ParseFilename("Zodiak-VariableItalic"));
    }

    [TestMethod]
    public void ParseFilename_CamelCaseBoldSuffix_RecognisedAsBold()
    {
        Assert.AreEqual(
            ("Zodiak", FontImporter.FontVariantClassification.Bold),
            FontImporter.ParseFilename("ZodiakBold"));
    }

    [TestMethod]
    public void ParseFilename_CamelCaseBoldItalicSuffix_RecognisedAsBoldItalic()
    {
        Assert.AreEqual(
            ("Zodiak", FontImporter.FontVariantClassification.BoldItalic),
            FontImporter.ParseFilename("ZodiakBoldItalic"));
    }

    [TestMethod]
    public void ParseFilename_CamelCaseLightSuffix_RecognisedAsOtherWeight()
    {
        Assert.AreEqual(
            ("Zodiak", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("ZodiakLight"));
    }

    [TestMethod]
    public void ParseFilename_AllUppercaseNoLowerToUpperBoundary_FallsThrough()
    {
        // No CamelCase boundary in an all-uppercase name, so no smushed suffix can be detected.
        // Falls through to the default (stem, Regular).
        Assert.AreEqual(
            ("ROBOTOBOLD", FontImporter.FontVariantClassification.Regular),
            FontImporter.ParseFilename("ROBOTOBOLD"));
    }

    // ── ParseFilename: family names with internal separators ──────────────

    [TestMethod]
    public void ParseFilename_FamilyWithUnderscoreOpticalSize_KeepsFamilyIntact()
    {
        // Google Sans Flex ships per-optical-size sub-families: GoogleSansFlex_24pt-Bold.ttf
        Assert.AreEqual(
            ("GoogleSansFlex_24pt", FontImporter.FontVariantClassification.Bold),
            FontImporter.ParseFilename("GoogleSansFlex_24pt-Bold"));
    }

    [TestMethod]
    public void ParseFilename_FamilyWithItalicWord_DoesNotMisclassify()
    {
        // "ItalicCaps" is a family name, not a style. Without a separator before, no suffix match.
        Assert.AreEqual(
            ("ItalicCaps", FontImporter.FontVariantClassification.Regular),
            FontImporter.ParseFilename("ItalicCaps"));
    }

    // ── ParseFilename: no-suffix fallback ─────────────────────────────────

    [TestMethod]
    public void ParseFilename_NoSuffix_ClassifiesAsRegularWithFullStem()
    {
        Assert.AreEqual(
            ("Lato", FontImporter.FontVariantClassification.Regular),
            FontImporter.ParseFilename("Lato"));
    }

    [TestMethod]
    public void ParseFilename_TimesNewRoman_KeepsFullFamilyName()
    {
        // "Roman" deliberately not treated as a style synonym — it collides with real family names.
        Assert.AreEqual(
            ("Times New Roman", FontImporter.FontVariantClassification.Regular),
            FontImporter.ParseFilename("Times New Roman"));
    }

    // ── ParseFilename: off-canonical weights ──────────────────────────────

    [TestMethod]
    public void ParseFilename_LightWeight_ClassifiesAsOtherWeight()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-Light"));
    }

    [TestMethod]
    public void ParseFilename_BlackWeight_ClassifiesAsOtherWeight()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-Black"));
    }

    [TestMethod]
    public void ParseFilename_ExtraBold_ClassifiesAsOtherWeight()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-ExtraBold"));
    }

    [TestMethod]
    public void ParseFilename_SemiBoldItalic_ClassifiesAsOtherWeight()
    {
        // SemiBold is off-canonical; the trailing italic doesn't promote it to one of our slots.
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-SemiBoldItalic"));
    }

    [TestMethod]
    public void ParseFilename_LightItalic_ClassifiesAsOtherWeight()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-LightItalic"));
    }

    [TestMethod]
    public void ParseFilename_ExtraBoldWithSpaceInside_ClassifiesAsOtherWeight()
    {
        Assert.AreEqual(
            ("Roboto", FontImporter.FontVariantClassification.OtherWeight),
            FontImporter.ParseFilename("Roboto-Extra Bold"));
    }

    // ── End-to-end Import ─────────────────────────────────────────────────

    [TestMethod]
    public void Import_FontPackWithAllFourVariants_CopiesAllAndReportsSources()
    {
        string src = MakeSourceFolder("Roboto-Regular.ttf", "Roboto-Bold.ttf", "Roboto-Italic.ttf", "Roboto-BoldItalic.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder());

        Assert.AreEqual("Roboto", result.TargetFontName);
        Assert.AreEqual("Roboto", result.SourceFamily);
        Assert.AreEqual("Roboto-Regular.ttf", result.RegularSource);
        Assert.AreEqual("Roboto-Bold.ttf", result.BoldSource);
        Assert.AreEqual("Roboto-Italic.ttf", result.ItalicSource);
        Assert.AreEqual("Roboto-BoldItalic.ttf", result.BoldItalicSource);
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "Roboto.ttf")));
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "Roboto_b.ttf")));
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "Roboto_i.ttf")));
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "Roboto_bi.ttf")));
    }

    [TestMethod]
    public void Import_FontPackWithOnlyRegularAndBold_LeavesItalicSlotsNull()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf", "Lato-Bold.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder());

        Assert.AreEqual("Lato-Regular.ttf", result.RegularSource);
        Assert.AreEqual("Lato-Bold.ttf", result.BoldSource);
        Assert.IsNull(result.ItalicSource);
        Assert.IsNull(result.BoldItalicSource);
        Assert.IsFalse(File.Exists(Path.Combine(DestFolder(), "Lato_i.ttf")));
        Assert.IsFalse(File.Exists(Path.Combine(DestFolder(), "Lato_bi.ttf")));
    }

    [TestMethod]
    public void Import_TargetFontNameOverride_RenamesDestinationFiles()
    {
        string src = MakeSourceFolder("GoogleSansFlex_24pt-Regular.ttf", "GoogleSansFlex_24pt-Bold.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder(), targetFontName: "GoogleSans");

        Assert.AreEqual("GoogleSans", result.TargetFontName);
        Assert.AreEqual("GoogleSansFlex_24pt", result.SourceFamily);
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "GoogleSans.ttf")));
        Assert.IsTrue(File.Exists(Path.Combine(DestFolder(), "GoogleSans_b.ttf")));
    }

    [TestMethod]
    public void Import_OffCanonicalWeightFiles_GoIntoSkippedList()
    {
        string src = MakeSourceFolder(
            "Roboto-Regular.ttf", "Roboto-Bold.ttf",
            "Roboto-Light.ttf", "Roboto-Black.ttf", "Roboto-Medium.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder());

        Assert.HasCount(3, result.SkippedFiles);
        Assert.Contains("Roboto-Light.ttf", result.SkippedFiles);
        Assert.Contains("Roboto-Black.ttf", result.SkippedFiles);
        Assert.Contains("Roboto-Medium.ttf", result.SkippedFiles);
    }

    [TestMethod]
    public void Import_MultipleFamilies_PicksOneWithMostVariantsAndListsOthers()
    {
        // Family A has 2 variants, family B has 4 → B wins.
        string src = MakeSourceFolder(
            "FamilyA-Regular.ttf", "FamilyA-Bold.ttf",
            "FamilyB-Regular.ttf", "FamilyB-Bold.ttf", "FamilyB-Italic.ttf", "FamilyB-BoldItalic.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder());

        Assert.AreEqual("FamilyB", result.SourceFamily);
        Assert.Contains("FamilyA", result.OtherFamilies);
    }

    [TestMethod]
    public void Import_FamilyFilter_RestrictsToMatchingFamily()
    {
        // Simulates Google Sans Flex pack with multiple optical sizes.
        string src = MakeSourceFolder(
            "GoogleSansFlex_24pt-Regular.ttf", "GoogleSansFlex_24pt-Bold.ttf",
            "GoogleSansFlex_36pt-Regular.ttf", "GoogleSansFlex_36pt-Bold.ttf",
            "GoogleSansFlex_72pt-Regular.ttf", "GoogleSansFlex_72pt-Bold.ttf");

        FontImporterResult result = FontImporter.Import(src, DestFolder(), familyFilter: "36pt");

        Assert.AreEqual("GoogleSansFlex_36pt", result.SourceFamily);
    }

    [TestMethod]
    public void Import_FamilyFilterMatchesNothing_Throws()
    {
        string src = MakeSourceFolder("Roboto-Regular.ttf", "Roboto-Bold.ttf");

        Assert.ThrowsExactly<InvalidOperationException>(
            () => FontImporter.Import(src, DestFolder(), familyFilter: "DoesNotExist"));
    }

    [TestMethod]
    public void Import_NonExistentSourceFolder_Throws()
    {
        Assert.ThrowsExactly<DirectoryNotFoundException>(
            () => FontImporter.Import(Path.Combine(_tempRoot, "no_such_folder"), DestFolder()));
    }

    [TestMethod]
    public void Import_EmptyFolder_Throws()
    {
        string src = MakeSourceFolder();

        Assert.ThrowsExactly<InvalidOperationException>(
            () => FontImporter.Import(src, DestFolder()));
    }

    [TestMethod]
    public void Import_DestinationDoesNotExist_CreatesIt()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf");
        string dest = Path.Combine(_tempRoot, "nested", "destination");
        Assert.IsFalse(Directory.Exists(dest));

        FontImporter.Import(src, dest);

        Assert.IsTrue(Directory.Exists(dest));
        Assert.IsTrue(File.Exists(Path.Combine(dest, "Lato.ttf")));
    }

    [TestMethod]
    public void Import_OverwriteTrue_ReplacesExistingDestinationFile()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf");
        Directory.CreateDirectory(DestFolder());
        string preExisting = Path.Combine(DestFolder(), "Lato.ttf");
        File.WriteAllText(preExisting, "stale");

        FontImporter.Import(src, DestFolder(), overwrite: true);

        Assert.AreEqual(0, new FileInfo(preExisting).Length);
    }

    [TestMethod]
    public void Preview_ReportsPlanButCopiesNothing()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf", "Lato-Bold.ttf");

        FontImporterResult result = FontImporter.Preview(src);

        Assert.AreEqual("Lato-Regular.ttf", result.RegularSource);
        Assert.AreEqual("Lato-Bold.ttf", result.BoldSource);
        // Preview must not touch the destination folder.
        Assert.IsFalse(Directory.Exists(DestFolder()));
    }

    [TestMethod]
    public void Preview_DoesNotOverwriteExistingFile()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf");
        Directory.CreateDirectory(DestFolder());
        string preExisting = Path.Combine(DestFolder(), "Lato.ttf");
        File.WriteAllText(preExisting, "preserved");

        FontImporter.Preview(src);

        Assert.AreEqual("preserved", File.ReadAllText(preExisting));
    }

    [TestMethod]
    public void FormatSummary_WithCustomVerb_UsesItInLeadLine()
    {
        // The CLI uses this to swap "imported" for "would import" on --dry-run.
        string src = MakeSourceFolder("Lato-Regular.ttf");
        FontImporterResult result = FontImporter.Preview(src);

        string summary = result.FormatSummary(verb: "would import");

        Assert.Contains("would import", summary);
        Assert.DoesNotContain("imported '", summary);
    }

    [TestMethod]
    public void Import_OverwriteFalseWithExistingFile_Throws()
    {
        string src = MakeSourceFolder("Lato-Regular.ttf");
        Directory.CreateDirectory(DestFolder());
        File.WriteAllText(Path.Combine(DestFolder(), "Lato.ttf"), "stale");

        Assert.ThrowsExactly<IOException>(
            () => FontImporter.Import(src, DestFolder(), overwrite: false));
    }

    [TestMethod]
    public void Import_GoogleSansFlexFullStaticFolder_DefaultPicksOneOpticalSize()
    {
        // Real-world: the static/ folder of a Google Sans Flex download has 4 optical sizes ×
        // 9 weights = 36 files. With the default settings, we expect the importer to pick ONE
        // optical size (the alphabetically first one when all have equal variant counts) and skip
        // every off-canonical weight from every family.
        string[] opticalSizes = ["24pt", "36pt", "72pt", "120pt"];
        string[] weights = ["Regular", "Bold", "Italic", "BoldItalic", "Light", "Medium", "SemiBold", "ExtraBold", "Black"];
        List<string> files = [];

        foreach (string opticalSize in opticalSizes)
        {
            foreach (string weight in weights)
            {
                files.Add($"GoogleSansFlex_{opticalSize}-{weight}.ttf");
            }
        }

        string src = MakeSourceFolder([..files]);

        FontImporterResult result = FontImporter.Import(src, DestFolder());

        Assert.AreEqual("GoogleSansFlex_120pt", result.SourceFamily);
        Assert.IsNotNull(result.RegularSource);
        Assert.IsNotNull(result.BoldSource);
        Assert.IsNotNull(result.ItalicSource);
        Assert.IsNotNull(result.BoldItalicSource);
        // 5 off-canonical weights × 4 optical sizes = 20 skipped files.
        Assert.HasCount(20, result.SkippedFiles);
        // The three other optical sizes are listed as other families.
        Assert.HasCount(3, result.OtherFamilies);
    }
}

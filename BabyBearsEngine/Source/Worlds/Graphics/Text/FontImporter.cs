using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using BabyBearsEngine.Diagnostics;

namespace BabyBearsEngine.Worlds.Graphics.Text;

/// <summary>
/// Summary of what <see cref="FontImporter.Import"/> picked out of a source font pack and copied
/// into <c>Assets/Fonts</c>. Each <c>*Source</c> property is the original source filename for the
/// slot, or <see langword="null"/> if that slot wasn't present in the source pack.
/// </summary>
public sealed record FontImporterResult(
    string TargetFontName,
    string SourceFamily,
    string? RegularSource,
    string? BoldSource,
    string? ItalicSource,
    string? BoldItalicSource,
    IReadOnlyList<string> SkippedFiles,
    IReadOnlyList<string> OtherFamilies)
{
    /// <summary>
    /// Produces a human-readable multi-line summary of the import. The lead verb is configurable
    /// so the same renderer works for both real imports (<c>"imported"</c>, the default) and
    /// dry-run previews (<c>"would import"</c>).
    /// </summary>
    public string FormatSummary(string verb = "imported")
    {
        StringBuilder sb = new();
        sb.AppendLine($"FontImporter: {verb} '{SourceFamily}' as '{TargetFontName}'");
        sb.AppendLine($"  Regular     ← {RegularSource ?? "(not found in source)"}");
        sb.AppendLine($"  Bold        ← {BoldSource ?? "(not found in source)"}");
        sb.AppendLine($"  Italic      ← {ItalicSource ?? "(not found in source)"}");
        sb.AppendLine($"  BoldItalic  ← {BoldItalicSource ?? "(not found in source)"}");

        if (SkippedFiles.Count > 0)
        {
            sb.AppendLine($"  Skipped {SkippedFiles.Count} file(s) with unsupported weight/style:");

            foreach (string fileName in SkippedFiles)
            {
                sb.AppendLine($"    - {fileName}");
            }
        }

        if (OtherFamilies.Count > 0)
        {
            sb.AppendLine($"  Other families in source (not imported): {string.Join(", ", OtherFamilies)}");
        }

        return sb.ToString().TrimEnd();
    }
}

/// <summary>
/// One-shot setup utility that takes a folder of .ttf files (e.g. the <c>static/</c> subfolder of a
/// Google Fonts download), picks out the Regular/Bold/Italic/BoldItalic variants by filename
/// pattern, and copies them into <c>Assets/Fonts</c> under the engine's convention:
/// <c>{Family}.ttf</c>, <c>{Family}_b.ttf</c>, <c>{Family}_i.ttf</c>, <c>{Family}_bi.ttf</c>.
/// Files with off-canonical weights (Light, Medium, SemiBold, ExtraBold, Black, etc.) are
/// recognised and skipped rather than misclassified as Regular.
/// </summary>
public static class FontImporter
{
    internal enum FontVariantClassification
    {
        Regular,
        Bold,
        Italic,
        BoldItalic,
        OtherWeight,
    }

    private const string DefaultDestinationFolder = "Assets/Fonts";

    // One or more separator chars between family and style (e.g. "Roboto-Bold", "Roboto_Bold",
    // "Roboto Bold"). Required between the family name and the style suffix.
    private const string SeparatorClass = @"[\s\-_]+";

    // Optional separator inside compound names like "Bold Italic", "Extra-Bold", "Semi_Bold".
    private const string OptionalSeparator = @"[\s\-_]*";

    private static readonly Regex s_boldItalicRegex = new(
        @"^(?<family>.+?)" + SeparatorClass + @"(?:bold" + OptionalSeparator + @"(?:italic|oblique)|(?:italic|oblique)" + OptionalSeparator + @"bold)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex s_boldRegex = new(
        @"^(?<family>.+?)" + SeparatorClass + @"bold$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex s_italicRegex = new(
        @"^(?<family>.+?)" + SeparatorClass + @"(?:italic|oblique)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // "Roman" and "Book" are deliberately omitted — they collide with real family-name parts
    // (e.g. "Times New Roman"). Users with those packs can override with targetFontName.
    private static readonly Regex s_regularRegex = new(
        @"^(?<family>.+?)" + SeparatorClass + @"(?:regular|normal)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex s_otherWeightRegex = new(
        @"^(?<family>.+?)" + SeparatorClass + @"(?:thin|hairline|extra" + OptionalSeparator + @"light|ultra" + OptionalSeparator + @"light|light|medium|semi" + OptionalSeparator + @"bold|demi" + OptionalSeparator + @"bold|extra" + OptionalSeparator + @"bold|ultra" + OptionalSeparator + @"bold|black|heavy)(?:" + OptionalSeparator + @"(?:italic|oblique))?$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Scans <paramref name="sourceFolder"/> for <c>.ttf</c> files, picks the Regular/Bold/Italic/
    /// BoldItalic variants by filename pattern, and copies them into <paramref name="destinationFolder"/>
    /// using the engine's convention.
    /// </summary>
    /// <param name="sourceFolder">Folder containing the source .ttf files (e.g. the <c>static/</c>
    /// subfolder of a Google Fonts download).</param>
    /// <param name="destinationFolder">Where the renamed files are written. Defaults to
    /// <c>Assets/Fonts</c> relative to the current working directory — when calling this as a
    /// one-off setup step, pass the absolute path to your project's source <c>Assets/Fonts</c>
    /// folder so the imports survive a build clean.</param>
    /// <param name="targetFontName">Override the destination family name. When <see langword="null"/>,
    /// the family is inferred from the source filenames (e.g. <c>"Roboto"</c> from
    /// <c>Roboto-Bold.ttf</c>).</param>
    /// <param name="familyFilter">When the source folder contains multiple families (e.g. Google
    /// Sans Flex ships several optical-size sub-families), only consider families whose name
    /// contains this substring. When <see langword="null"/>, the family with the most variants is
    /// picked; ties go to the alphabetically-first name.</param>
    /// <param name="overwrite">When <see langword="true"/> (default), destination files are
    /// overwritten if they already exist.</param>
    /// <returns>A <see cref="FontImporterResult"/> describing which slots were filled and what was
    /// skipped.</returns>
    public static FontImporterResult Import(
        string sourceFolder,
        string destinationFolder = DefaultDestinationFolder,
        string? targetFontName = null,
        string? familyFilter = null,
        bool overwrite = true)
    {
        ArgumentException.ThrowIfNullOrEmpty(destinationFolder);

        FontImporterPlan plan = DiscoverPlan(sourceFolder, targetFontName, familyFilter);

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
        }

        CopySlot(plan, FontVariantClassification.Regular, destinationFolder, suffix: "", overwrite);
        CopySlot(plan, FontVariantClassification.Bold, destinationFolder, suffix: "_b", overwrite);
        CopySlot(plan, FontVariantClassification.Italic, destinationFolder, suffix: "_i", overwrite);
        CopySlot(plan, FontVariantClassification.BoldItalic, destinationFolder, suffix: "_bi", overwrite);

        FontImporterResult result = PlanToResult(plan);
        Logger.Info(result.FormatSummary());
        return result;
    }

    /// <summary>
    /// Runs the discovery and family-selection pass without copying anything. Returns the same
    /// shape of result <see cref="Import"/> would, so callers can preview which files would land
    /// in which slot before committing to the copy. Throws the same exceptions as
    /// <see cref="Import"/> for invalid source folders or no-matching-files conditions.
    /// </summary>
    public static FontImporterResult Preview(
        string sourceFolder,
        string? targetFontName = null,
        string? familyFilter = null)
    {
        FontImporterPlan plan = DiscoverPlan(sourceFolder, targetFontName, familyFilter);
        return PlanToResult(plan);
    }

    /// <summary>
    /// Resolved plan from a discovery pass: which family was picked, which file fills which slot
    /// (by full path, for the copy step), and what was skipped or left over. Shared by
    /// <see cref="Import"/> and <see cref="Preview"/> so both surface the same selection logic.
    /// </summary>
    private sealed record FontImporterPlan(
        string TargetFontName,
        string SourceFamily,
        IReadOnlyDictionary<FontVariantClassification, string> Slots,
        IReadOnlyList<string> SkippedFiles,
        IReadOnlyList<string> OtherFamilies);

    private static FontImporterPlan DiscoverPlan(string sourceFolder, string? targetFontName, string? familyFilter)
    {
        ArgumentException.ThrowIfNullOrEmpty(sourceFolder);

        if (!Directory.Exists(sourceFolder))
        {
            throw new DirectoryNotFoundException($"FontImporter: source folder '{sourceFolder}' does not exist.");
        }

        // family -> (variant -> source path). First file wins for each (family, variant) slot.
        Dictionary<string, Dictionary<FontVariantClassification, string>> families = new(StringComparer.OrdinalIgnoreCase);
        List<string> skippedFiles = [];

        foreach (string filePath in Directory.GetFiles(sourceFolder, "*.ttf", SearchOption.TopDirectoryOnly))
        {
            string stem = Path.GetFileNameWithoutExtension(filePath);
            (string family, FontVariantClassification variant) = ParseFilename(stem);

            if (variant == FontVariantClassification.OtherWeight)
            {
                skippedFiles.Add(Path.GetFileName(filePath));
                continue;
            }

            if (!families.TryGetValue(family, out Dictionary<FontVariantClassification, string>? slots))
            {
                slots = new Dictionary<FontVariantClassification, string>();
                families[family] = slots;
            }

            if (!slots.ContainsKey(variant))
            {
                slots[variant] = filePath;
            }
        }

        if (families.Count == 0)
        {
            throw new InvalidOperationException(
                $"FontImporter: no .ttf files in '{sourceFolder}' matched a Regular/Bold/Italic/BoldItalic naming pattern.");
        }

        // Pick a family: optionally filter, then prefer the one with the most variants, ties to
        // the alphabetically-first name.
        var candidates = familyFilter is null
            ? families.AsEnumerable()
            : families.Where(p => p.Key.Contains(familyFilter, StringComparison.OrdinalIgnoreCase));

        var picked = candidates
            .OrderByDescending(p => p.Value.Count)
            .ThenBy(p => p.Key, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();

        if (picked.Key is null)
        {
            throw new InvalidOperationException(
                $"FontImporter: no families in '{sourceFolder}' matched filter '{familyFilter}'. Found: {string.Join(", ", families.Keys)}");
        }

        string sourceFamily = picked.Key;
        Dictionary<FontVariantClassification, string> slotsToImport = picked.Value;
        string finalTargetName = targetFontName ?? sourceFamily;

        var otherFamilies = families.Keys
            .Where(name => !string.Equals(name, sourceFamily, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new FontImporterPlan(
            TargetFontName: finalTargetName,
            SourceFamily: sourceFamily,
            Slots: slotsToImport,
            SkippedFiles: skippedFiles,
            OtherFamilies: otherFamilies);
    }

    private static FontImporterResult PlanToResult(FontImporterPlan plan)
    {
        string? FilenameFor(FontVariantClassification variant) =>
            plan.Slots.TryGetValue(variant, out string? path) ? Path.GetFileName(path) : null;

        return new FontImporterResult(
            TargetFontName: plan.TargetFontName,
            SourceFamily: plan.SourceFamily,
            RegularSource: FilenameFor(FontVariantClassification.Regular),
            BoldSource: FilenameFor(FontVariantClassification.Bold),
            ItalicSource: FilenameFor(FontVariantClassification.Italic),
            BoldItalicSource: FilenameFor(FontVariantClassification.BoldItalic),
            SkippedFiles: plan.SkippedFiles,
            OtherFamilies: plan.OtherFamilies);
    }

    private static void CopySlot(
        FontImporterPlan plan,
        FontVariantClassification variant,
        string destinationFolder,
        string suffix,
        bool overwrite)
    {
        if (!plan.Slots.TryGetValue(variant, out string? sourcePath))
        {
            return;
        }

        string destinationPath = Path.Combine(destinationFolder, plan.TargetFontName + suffix + ".ttf");
        File.Copy(sourcePath, destinationPath, overwrite);
    }

    /// <summary>
    /// Classifies a font filename stem (no extension) by inspecting its style suffix. Returns the
    /// inferred family name (everything before the style) and the recognised variant. When no
    /// known suffix is found, the whole stem is taken as the family and the variant defaults to
    /// <see cref="FontVariantClassification.Regular"/> — this handles files like <c>"Lato.ttf"</c>
    /// that ship without a style suffix.
    /// </summary>
    internal static (string Family, FontVariantClassification Variant) ParseFilename(string stem)
    {
        // Off-canonical weights (Light/Medium/Black/etc.) must be checked first so that, for
        // example, "Roboto-Light-Italic" classifies as OtherWeight rather than letting the
        // canonical Italic regex strip just "Italic" and call the family "Roboto-Light".
        Match match = s_otherWeightRegex.Match(stem);
        if (match.Success)
        {
            return (CleanFamilyName(match.Groups["family"].Value), FontVariantClassification.OtherWeight);
        }

        match = s_boldItalicRegex.Match(stem);
        if (match.Success)
        {
            return (CleanFamilyName(match.Groups["family"].Value), FontVariantClassification.BoldItalic);
        }

        match = s_boldRegex.Match(stem);
        if (match.Success)
        {
            return (CleanFamilyName(match.Groups["family"].Value), FontVariantClassification.Bold);
        }

        match = s_italicRegex.Match(stem);
        if (match.Success)
        {
            return (CleanFamilyName(match.Groups["family"].Value), FontVariantClassification.Italic);
        }

        match = s_regularRegex.Match(stem);
        if (match.Success)
        {
            return (CleanFamilyName(match.Groups["family"].Value), FontVariantClassification.Regular);
        }

        return (stem, FontVariantClassification.Regular);
    }

    private static string CleanFamilyName(string family) => family.TrimEnd(' ', '-', '_');
}

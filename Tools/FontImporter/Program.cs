using System;
using System.Collections.Generic;
using System.IO;
using BabyBearsEngine.Worlds.Graphics.Text;

namespace BabyBearsEngine.Tools.FontImporter;

internal static class Program
{
    private const string HelpText =
        """
        Usage:
          dotnet run --project Tools/FontImporter -- <source-folder> [options]

        Arguments:
          <source-folder>          Folder containing the .ttf files to import. Typically the
                                   `static/` subfolder of a Google Fonts download.

        Options:
          --to <path>              Destination folder. Defaults to "Assets/Fonts" relative to
                                   the current working directory. Pass an absolute path to your
                                   project's source Assets/Fonts folder so imports survive a clean.
          --name <name>            Override the destination family name. By default the family
                                   is inferred from the source filenames (e.g. "Roboto" from
                                   "Roboto-Bold.ttf").
          --filter <substring>     When the source contains several families (e.g. Google Sans
                                   Flex's per-optical-size sub-families), only consider those
                                   whose name contains this substring (e.g. "24pt").
          --no-overwrite           Don't overwrite existing destination files. The import will
                                   abort if a destination already exists. Overwrite is the
                                   default to make re-imports idempotent.
          --dry-run                Print what would be imported without copying anything.
          -h, --help               Print this help and exit.

        Examples:
          # Import the 24pt cut of Google Sans Flex as "GoogleSansFlex".
          dotnet run --project Tools/FontImporter -- \
            "C:\Users\me\Downloads\Google_Sans_Flex\static" \
            --to "BabyBearsEngine\Assets\Fonts" \
            --name GoogleSansFlex \
            --filter 24pt

          # Preview a Roboto import without writing anything.
          dotnet run --project Tools/FontImporter -- \
            "C:\Users\me\Downloads\Roboto" --dry-run
        """;

    public static int Main(string[] args)
    {
        if (args.Length == 0 || HasFlag(args, "-h") || HasFlag(args, "--help"))
        {
            Console.WriteLine(HelpText);
            return args.Length == 0 ? 1 : 0;
        }

        ParsedArgs parsed;

        try
        {
            parsed = ParseArgs(args);
        }
        catch (ArgumentException argEx)
        {
            Console.Error.WriteLine($"FontImporter: {argEx.Message}");
            Console.Error.WriteLine("Run with --help for usage.");
            return 2;
        }

        try
        {
            FontImporterResult result;

            if (parsed.DryRun)
            {
                Console.WriteLine("DRY RUN — no files will be copied. Re-run without --dry-run to commit.");
                Console.WriteLine();

                result = global::BabyBearsEngine.Worlds.Graphics.Text.FontImporter.Preview(
                    sourceFolder: parsed.SourceFolder,
                    targetFontName: parsed.TargetFontName,
                    familyFilter: parsed.FamilyFilter);

                Console.WriteLine(result.FormatSummary(verb: "would import"));
            }
            else
            {
                result = global::BabyBearsEngine.Worlds.Graphics.Text.FontImporter.Import(
                    sourceFolder: parsed.SourceFolder,
                    destinationFolder: parsed.DestinationFolder,
                    targetFontName: parsed.TargetFontName,
                    familyFilter: parsed.FamilyFilter,
                    overwrite: parsed.Overwrite);

                Console.WriteLine(result.FormatSummary());
            }

            return 0;
        }
        catch (DirectoryNotFoundException dirEx)
        {
            Console.Error.WriteLine(dirEx.Message);
            return 3;
        }
        catch (InvalidOperationException invEx)
        {
            Console.Error.WriteLine(invEx.Message);
            return 4;
        }
        catch (IOException ioEx)
        {
            Console.Error.WriteLine($"FontImporter: {ioEx.Message}");
            Console.Error.WriteLine("(Overwrite is on by default; pass --no-overwrite only if you want the existing files preserved.)");
            return 5;
        }
    }

    private sealed record ParsedArgs(
        string SourceFolder,
        string DestinationFolder,
        string? TargetFontName,
        string? FamilyFilter,
        bool Overwrite,
        bool DryRun);

    private static ParsedArgs ParseArgs(string[] args)
    {
        string? sourceFolder = null;
        string destinationFolder = "Assets/Fonts";
        string? targetFontName = null;
        string? familyFilter = null;
        bool overwrite = true;
        bool dryRun = false;

        for (int argIndex = 0; argIndex < args.Length; argIndex++)
        {
            string arg = args[argIndex];

            switch (arg)
            {
                case "--to":
                    destinationFolder = RequireValue(args, ref argIndex, arg);
                    break;
                case "--name":
                    targetFontName = RequireValue(args, ref argIndex, arg);
                    break;
                case "--filter":
                    familyFilter = RequireValue(args, ref argIndex, arg);
                    break;
                case "--no-overwrite":
                    overwrite = false;
                    break;
                case "--dry-run":
                    dryRun = true;
                    break;
                case "-h":
                case "--help":
                    // Handled in Main before ParseArgs is called; tolerate a stray occurrence here.
                    break;
                default:
                    if (arg.StartsWith('-'))
                    {
                        throw new ArgumentException($"unknown option '{arg}'");
                    }

                    if (sourceFolder is not null)
                    {
                        throw new ArgumentException($"unexpected extra positional argument '{arg}' (source folder already set to '{sourceFolder}')");
                    }

                    sourceFolder = arg;
                    break;
            }
        }

        if (sourceFolder is null)
        {
            throw new ArgumentException("missing required <source-folder> argument");
        }

        return new ParsedArgs(sourceFolder, destinationFolder, targetFontName, familyFilter, overwrite, dryRun);
    }

    private static string RequireValue(string[] args, ref int argIndex, string optionName)
    {
        if (argIndex + 1 >= args.Length)
        {
            throw new ArgumentException($"option '{optionName}' requires a value");
        }

        argIndex++;
        return args[argIndex];
    }

    private static bool HasFlag(string[] args, string flag)
    {
        foreach (string arg in args)
        {
            if (arg == flag)
            {
                return true;
            }
        }

        return false;
    }
}

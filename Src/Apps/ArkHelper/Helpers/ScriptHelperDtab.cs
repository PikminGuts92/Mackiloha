using ArkHelper.Exceptions;
using CliWrap;
using Mackiloha.DTB;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ArkHelper.Helpers;

public class ScriptHelperDtab : IScriptHelper
{
    protected string DtabPath;

    protected readonly Encoding Encoding;
    protected readonly Regex IndentRegex;

    public ScriptHelperDtab(ILogManager _)
    {
        DtabPath = $"dtab{GetExeExtension()}";
        Encoding = Encoding.UTF8;
        IndentRegex = new Regex(@"^[\s]+");
    }

    public void Initialize()
    {
        DtabPath = ResolveDtabPath();
    }

    protected virtual string ResolveDtabPath()
    {
        var dtabFileName = $"dtab{GetExeExtension()}";
        var dtabPath = Path.Combine(GetExeDirectory(), dtabFileName);

        // Check if dtab exists relative to exe
        if (File.Exists(dtabPath))
        {
            Log.Information("Using {DtabFileName} relative to executable", dtabFileName);
            return dtabPath;
        }

        Log.Information("Using {DtabFileName} in Path environment variable", dtabFileName);
        return dtabFileName;
    }

    protected virtual string GetExeExtension()
        => RuntimeInformation.OSDescription.Contains("Windows") ? ".exe" : "";

    protected virtual string GetExeDirectory()
        => AppContext.BaseDirectory;

    protected virtual void WriteOutput(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }
        else if (text.Contains("Parse error")
            || text.Contains("CallStack")
            || text.Contains("error, called at"))
        {
            WriteOutputError(text);
            return;
        };

        Log.Information(text);
    }

    protected virtual void WriteOutputError(string text)
        => Log.Error(text);

    public virtual void ConvertNewDtbToOld(string newDtbPath, string oldDtbPath, bool fme = false)
    {
        var encoding = fme ? DTBEncoding.FME : DTBEncoding.RBVR;

        var dtb = DTBFile.FromFile(newDtbPath, encoding);
        dtb.Encoding = DTBEncoding.Classic;
        dtb.SaveToFile(oldDtbPath);
    }

    public virtual string ConvertDtbToDta(string dtbPath, string tempDir, bool newEncryption, int arkVersion, string dtaPath = null, int indentSize = 3)
    {
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        var decDtbPath = Path.Combine(tempDir, Path.GetRandomFileName());
        dtaPath = dtaPath ?? Path.Combine(tempDir, Path.GetRandomFileName());

        var dtaDir = Path.GetDirectoryName(dtaPath);
        if (!Directory.Exists(dtaDir))
            Directory.CreateDirectory(dtaDir);

        if (arkVersion < 8)
        {
            // Decrypt dtb
            Cli.Wrap(DtabPath)
                .WithArguments(new[]
                {
                    newEncryption ? "-d" : "-D",
                    dtbPath,
                    decDtbPath
                })
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(WriteOutput))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(WriteOutputError))
                .ExecuteAsync()
                .Task.Wait();
        }
        else
        {
            // New dtb style, convert to old format to use dtab
            //  and assume not encrypted
            ConvertNewDtbToOld(dtbPath, decDtbPath, arkVersion < 9);
        }

        // Convert to dta (plaintext)
        var result = Cli.Wrap(DtabPath)
            .WithArguments(new[]
            {
                arkVersion == 7 ? "-A" : "-a",
                decDtbPath,
                dtaPath
            })
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(WriteOutput))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(WriteOutput))
            .ExecuteAsync()
            .Task.Result;

        if (result.ExitCode != 0)
            throw new DTBParseException($"dtab.exe was unable to parse file from \'{decDtbPath}\'");

        // Update dta indention
        UpdateTabIndention(dtaPath, dtaPath, indentSize);
        return dtaPath;
    }

    public virtual void ConvertOldDtbToNew(string oldDtbPath, string newDtbPath, bool fme = false)
    {
        var encoding = fme ? DTBEncoding.FME : DTBEncoding.RBVR;

        var dtb = DTBFile.FromFile(oldDtbPath, DTBEncoding.Classic);
        dtb.Encoding = encoding;
        dtb.SaveToFile(newDtbPath);
    }

    public virtual string ConvertDtaToDtb(string dtaPath, string tempDir, bool newEncryption, int arkVersion)
    {
        if (!Directory.Exists(tempDir))
            Directory.CreateDirectory(tempDir);

        var dtbPath = Path.Combine(tempDir, Path.GetRandomFileName());
        var encDtbPath = Path.Combine(tempDir, Path.GetRandomFileName());

        // Convert to dtb
        var result = Cli.Wrap(DtabPath)
            .WithArguments(new[]
            {
                "-b",
                dtaPath,
                dtbPath
            })
            .WithValidation(CommandResultValidation.None)
            .WithStandardOutputPipe(PipeTarget.ToDelegate(WriteOutput))
            .WithStandardErrorPipe(PipeTarget.ToDelegate(WriteOutput))
            .ExecuteAsync()
            .Task.Result;

        if (result.ExitCode != 0)
            throw new DTBParseException($"dtab.exe was unable to parse file from \'{dtaPath}\'");

        if (arkVersion < 7)
        {
            // Encrypt dtb (binary)
            Cli.Wrap(DtabPath)
                .WithArguments(new[]
                {
                    newEncryption ? "-e" : "-E",
                    dtbPath,
                    encDtbPath
                })
                .WithValidation(CommandResultValidation.None)
                .WithStandardOutputPipe(PipeTarget.ToDelegate(WriteOutput))
                .WithStandardErrorPipe(PipeTarget.ToDelegate(WriteOutput))
                .ExecuteAsync()
                .Task.Wait();
        }
        else
        {
            // Convert
            ConvertOldDtbToNew(dtbPath, encDtbPath, arkVersion < 9);
        }

        return encDtbPath;
    }

    protected void UpdateTabIndention_Orig(string inputDta, string outputDta, int indentSize = 3)
    {
        var lines = File.ReadAllLines(inputDta, Encoding)
            .Select(line => IndentRegex
                .Replace(line, new string(' ', IndentRegex.Match(line).Length * indentSize)))
            .ToArray();

        File.WriteAllLines(outputDta, lines);
    }

    public void UpdateTabIndention(string inputDta, string outputDta, int indentSize = 3)
    {
        // Insert spaces without affecting encoding
        const byte NEWLINE = 0x0A;
        const byte SPACE = 0x20;

        var inData = File.ReadAllBytes(inputDta);
        using var bw = new BinaryWriter(new MemoryStream());

        int i = 0;
        while (i < inData.Length)
        {
            var prependStart = i;
            var spaceCount = 0;

            // Skip newlines
            while (i < inData.Length
                && inData[i] == NEWLINE) i++;

            // Count spaces
            while (i < inData.Length
                && inData[i] == SPACE)
            {
                spaceCount++;
                i++;
            }

            // Write prepend text
            if (prependStart < i)
            {
                bw.Write(inData[prependStart..i]);
            }

            // Write extra spaces
            if (spaceCount > 0
                && indentSize > 1)
            {
                var extraSpaceSize = (indentSize * spaceCount) - spaceCount;
                var extraSpaces = Enumerable
                    .Range(0, extraSpaceSize)
                    .Select(x => SPACE)
                    .ToArray();

                bw.Write(extraSpaces);
            }

            var appendStart = i;

            // Count until newline or eof
            while (i < inData.Length
                && inData[i] != NEWLINE) i++;

            // Write append text
            if (appendStart < i)
            {
                bw.Write(inData[appendStart..i]);
            }
        }

        // Write to file
        File.WriteAllBytes(outputDta, ((MemoryStream)bw.BaseStream).ToArray());
    }
}

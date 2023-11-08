namespace Mackiloha.Ark;

public class PendingArkEntry : ArkEntry
{
    public PendingArkEntry(string fileName, string directory) : base(fileName, directory)
    {

    }

    public PendingArkEntry(PendingArkEntry entry) : this(entry.FileName, entry.Directory)
    {
        LocalFilePath = entry.LocalFilePath;
    }

    public string LocalFilePath { get; set; }
}

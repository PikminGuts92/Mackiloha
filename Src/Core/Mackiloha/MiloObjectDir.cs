namespace Mackiloha;

public class MiloObjectDir : MiloObject
{
    private string _name;

    public override string Name
    {
        get => _name ?? GetDirectoryEntry()?.Name;
        set => _name = value;
    }

    public List<MiloObject> Entries { get; } = new List<MiloObject>();

    // TODO: Change object to ISerializable
    public Dictionary<string, object> Extras { get; } = new Dictionary<string, object>();

    public override string Type => GetDirectoryEntry()?.Type ?? "ObjectDir";

    public MiloObject GetDirectoryEntry()
    {
        if (Extras.TryGetValue("DirectoryEntry", out var dirEntry))
        {
            return dirEntry as MiloObject;
        }

        return null;
    }
}

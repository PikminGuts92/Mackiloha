namespace ArkHelper.Models
{
    public class ArkCache
    {
        public int Version { get; set; }
        public bool Encrypted { get; set; }
        public List<CachedFileInfo> Files { get; set; }
    }
}

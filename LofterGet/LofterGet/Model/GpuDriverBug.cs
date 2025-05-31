using System.Text.Json.Serialization;

namespace LofterGet.Model;

[JsonSerializable(typeof(GpuDriverBugList))]
internal partial class SrcGenContext : JsonSerializerContext { }

internal class GpuDriverBugEntry
{
    public int id { get; set; }
    public string description { get; set; }
    public string[] features { get; set; }
    public OsInfo os { get; set; }
}

internal class OsInfo
{
    public string type { get; set; }
}

internal class GpuDriverBugList
{
    public string name { get; set; }
    public GpuDriverBugEntry[] entries { get; set; }
}

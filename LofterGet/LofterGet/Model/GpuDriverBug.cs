using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LofterGet.Model;

[JsonSerializable(typeof(GpuDriverBugList))]
internal partial class SrcGenContext : JsonSerializerContext { }

internal class GpuDriverBugEntry
{
    public int id { get; set; }
    public string description { get; set; }
    public string[] features { get; set; }
}

internal class GpuDriverBugList
{
    public string name { get; set; }
    public GpuDriverBugEntry[] entries { get; set; }
}

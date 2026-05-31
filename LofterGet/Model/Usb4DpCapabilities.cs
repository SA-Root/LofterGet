using System.ComponentModel;

namespace LofterGet.Model;

internal enum Usb4DpCapProtocolAdapterVersion
{
    [Description("Thunderbolt 3")]
    Thunderbolt3 = 3,
    [Description("USB4 v1.0")]
    USB4v1 = 4,
    [Description("USB4 v2.0")]
    USB4v2 = 5,
}

internal enum Usb4DpcdVersion
{
    [Description("r1.1")]
    r1_1 = 0,
    [Description("r1.2")]
    r1_2 = 1,
    [Description("r1.3")]
    r1_3 = 2,
    [Description("r1.4a")]
    r1_4a = 3,
}

internal enum Usb4DpMaxLinkRate8b10b
{
    [Description("1.62 Gbps/lane")]
    R1_62Gbps = 0,
    [Description("2.7 Gbps/lane")]
    R2_7Gbps = 1,
    [Description("5.4 Gbps/lane")]
    R5_4Gbps = 2,
    [Description("8.1 Gbps/lane")]
    R8_1Gbps = 3,
}
    
internal enum Usb4DpMaxLaneCount
{
    [Description("1 lane")]
    L1 = 0,
    [Description("2 lanes")]
    L2 = 1,
    [Description("4 lanes")]
    L4 = 2,
}

internal class Usb4DpCapabilities
{
    public int RawValue { get; set; }

    public string Usb4Version =>
        ((Usb4DpCapProtocolAdapterVersion)(RawValue & 0b1111)).GetDescription();

    public string DpcdVersion =>
        ((Usb4DpcdVersion)((RawValue >> 4) & 0b1111)).GetDescription();

    public string MaxLinkRate8b10b =>
        ((Usb4DpMaxLinkRate8b10b)((RawValue >> 8) & 0b1111)).GetDescription();

    public string MaxLaneCount =>
        ((Usb4DpMaxLaneCount)((RawValue >> 12) & 0b111)).GetDescription();

    /// <summary>
    /// 8b/10b MST Capability
    /// </summary>
    public bool MstCap8b10b => ((RawValue >> 15) & 0b1) == 1;
    
    /// <summary>
    /// 128b/132b Link Layer & 10 Gbps/Lane Support
    /// </summary>
    public bool LL128b132b => ((RawValue >> 17) & 0b1) == 1;

    /// <summary>
    /// 20 Gbps/Lane Support
    /// </summary>
    public bool Lane20Gbps => ((RawValue >> 18) & 0b1) == 1;

    /// <summary>
    /// 13.5 Gbps/Lane Support
    /// </summary>
    public bool Lane13_5Gbps => ((RawValue >> 19) & 0b1) == 1;

    /// <summary>
    /// DSC Not Supported
    /// </summary>
    public bool DscNotSupported => ((RawValue >> 29) & 0b1) == 1;
}

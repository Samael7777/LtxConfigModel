using System.Diagnostics;

namespace LtxConfigModel.LineElements;

/// <summary>
/// Represents a key token in a key/value line. Serialized as: [Padding.Left]key[Padding.Right]=
/// </summary>
[DebuggerDisplay("Key: {Key}")]
public class KeyElement(string key, Padding padding = default) : LineElement(padding)
{
    /// <summary>The key text (identifier) used on the left side of a key/value pair.</summary>
    public string Key { get; set; } = key;

    public override string Serialize()
        => $"{Padding.LeftStr}{Key}{Padding.RightStr}=";
}

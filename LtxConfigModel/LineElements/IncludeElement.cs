using System.Diagnostics;

namespace LtxConfigModel.LineElements;

/// <summary>
/// Represents an include directive (#include "path/to/file.ltx").
/// Padding is applied around the directive when serializing.
/// </summary>
[DebuggerDisplay("#include \"{Path}\"")]
public class IncludeElement(string path, Padding padding = default) : LineElement(padding)
{
    /// <summary>Relative or absolute path specified in the include directive.</summary>
    public string Path { get; set; } = path;

    public override string Serialize()
        => $"{Padding.LeftStr}#include \"{Path}\"{Padding.RightStr}";
}

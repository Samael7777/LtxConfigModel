using System.Diagnostics;

namespace LtxConfigModel.LineElements;

/// <summary>
/// Represents a trailing comment element. Serialized as: [Padding.Left];text
/// Padding.Left controls spaces before the comment marker (';').
/// </summary>
[DebuggerDisplay(";{Text}")]
public class CommentElement(string text, Padding padding = default) : LineElement(padding)
{
    /// <summary>Comment text without the leading ';' character.</summary>
    public string Text { get; set; } = text;

n    public override string Serialize()
        => $"{Padding.LeftStr};{Text}";
}

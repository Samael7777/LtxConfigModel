using System.Diagnostics;

namespace LtxConfigModel.LineElements;

/// <summary>
/// Represents a trailing comment element. Serialized as: [Padding.Left];text
/// Padding.Left controls spaces before the comment marker (';').
/// </summary>
[DebuggerDisplay(";{Text}")]
public class CommentElement : LineElement
{
    /// <summary>
    /// Represents a trailing comment element. Serialized as: [Padding.Left];text
    /// Padding.Left controls spaces before the comment marker (';').
    /// </summary>
    public CommentElement(string text, Padding padding = default) : base(padding)
    {
        Text = text;
    }

    /// <summary>Comment text without the leading ';' character.</summary>
    public string Text { get; set; }

    public override string Serialize()
        => $"{Padding.LeftStr};{Text}";
}

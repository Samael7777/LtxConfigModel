namespace LtxConfigModel.LineElements;

/// <summary>
/// Represents a single token in a possibly comma-separated value list.
/// Padding.Left represents spaces after the previous comma (or after '=' for the first item).
/// Padding.Right represents spaces before the next comma (or before end of line/comment for the last item).
/// Example serialization: key = [L]val1[R],[L]val2[R],[L]val3[R]
/// </summary>
public class ValueItem
{
    /// <summary>Create a ValueItem specifying left and right padding counts.</summary>
    public ValueItem(string value, int leftPadding, int rightPadding) :
        this(value, new Padding(leftPadding, rightPadding))
    { }

    /// <summary>
    /// Represents a single token in a possibly comma-separated value list.
    /// Padding.Left represents spaces after the previous comma (or after '=' for the first item).
    /// Padding.Right represents spaces before the next comma (or before end of line/comment for the last item).
    /// Example serialization: key = [L]val1[R],[L]val2[R],[L]val3[R]
    /// </summary>
    public ValueItem(string value, Padding padding = default)
    {
        Value = value;
        Padding = padding;
    }


    /// <summary>The plain value text.</summary>
    public string  Value   { get; set; }

    /// <summary>Padding around this token when serialized.</summary>
    public Padding Padding { get; set; }

    /// <summary>Serialize the token including its padding.</summary>
    public string Serialize() => $"{Padding.LeftStr}{Value}{Padding.RightStr}";
}

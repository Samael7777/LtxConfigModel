namespace LtxConfigModel.LineElements;

internal class UnparsedLine
{
    public InlineDataElement? DataElement { get; }
    public CommentElement? CommentElement { get; }
    public int RawLineNumber { get; }

    public UnparsedLine(string line, int rawLineNumber)
    {
        var idx1 = line.IndexOf(';');
        var idx2 = line.IndexOf("//", StringComparison.Ordinal);
        
        var parts = idx1 == -1 || (idx2 != -1 && idx2 < idx1)
            ? line.Split("//", 2)
            : line.Split(';', 2);

        var rawData = string.IsNullOrWhiteSpace(parts[0]) ? null : parts[0];
        var comment = parts.Length == 2
            ? string.IsNullOrWhiteSpace(parts[1]) ? null : parts[1]
            : null;

        DataElement = BuildInlineDataElement(rawData);
        CommentElement = BuildCommentElement(comment);
        RawLineNumber = rawLineNumber;
    }

    private static CommentElement? BuildCommentElement(string? text)
    {
        return string.IsNullOrWhiteSpace(text) 
            ? null 
            : new CommentElement(text.TrimEnd(), new Padding(text.LeftPadding(), 0));
    }

    private static InlineDataElement? BuildInlineDataElement(string? text)
    {
        return string.IsNullOrWhiteSpace(text) 
            ? null 
            : new InlineDataElement(text.Trim(), text.LeftPadding(), text.RightPadding());
    }
}
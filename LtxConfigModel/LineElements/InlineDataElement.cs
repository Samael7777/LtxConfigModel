namespace LtxConfigModel.LineElements;

internal class InlineDataElement(string text, int leftPadding = 0, int rightPadding = 0): 
    LineElement(new Padding(leftPadding, rightPadding))
{
    public string Text { get; } = text;

    public override string Serialize() => $"{Padding.LeftStr}{Text}{Padding.RightStr}";
}
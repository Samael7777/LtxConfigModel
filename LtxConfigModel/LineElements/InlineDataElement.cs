namespace LtxConfigModel.LineElements;

internal class InlineDataElement : 
    LineElement
{
    public InlineDataElement(string text, int leftPadding = 0, int rightPadding = 0) : base(new Padding(leftPadding, rightPadding))
    {
        Text = text;
    }

    public string Text { get; }

    public override string Serialize() => $"{Padding.LeftStr}{Text}{Padding.RightStr}";
}
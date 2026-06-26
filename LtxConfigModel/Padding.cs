namespace LtxConfigModel;

/// <summary>
/// Represents counts of spaces (padding) applied to the left and right of tokens
/// when serializing or parsing lines. Semantics depend on the token type:
/// - Key: Left = indent from line start, Right = spaces before '='.
/// - Value: Left = spaces after '=', Right = spaces before end/comment.
/// - Section header: Left/Right = spaces inside '[' and ']'.
/// </summary>
public readonly struct Padding(int left, int right)
{
    /// <summary>Predefined value for no padding (0,0).</summary>
    public static readonly Padding None = new(0, 0);

    /// <summary>Number of spaces on the left side.</summary>
    public int Left  { get; } = left;
    /// <summary>Number of spaces on the right side.</summary>
    public int Right { get; } = right;

    /// <summary>Left padding rendered as a string of spaces.</summary>
    public string LeftStr  => new (' ', Left);
    /// <summary>Right padding rendered as a string of spaces.</summary>
    public string RightStr => new (' ', Right);
}

namespace LtxConfigModel.LineElements;

/// <summary>
/// Base class for elements that can appear inside a Line (key, value, comment, include, etc.).
/// Provides a Padding property and requires a Serialize implementation that converts
/// the element back to its textual representation.
/// </summary>
public abstract class LineElement(Padding padding = default)
{
    /// <summary>Padding applied around this element when serializing.</summary>
    public Padding Padding { get; set; } = padding;

    /// <summary>Serialize the element to its textual representation, including padding.</summary>
    public abstract string Serialize();
}

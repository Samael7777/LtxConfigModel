using System.Diagnostics;
using System.Text;
using LtxConfigModel.LineElements;

namespace LtxConfigModel;

/// <summary>
/// Represents a single line of a .ltx configuration file. A line can be empty (no elements)
/// or contain an ordered collection of LineElement instances that make up the line
/// (for example: KeyElement, ValueElement, CommentElement, IncludeElement).
/// </summary>

[DebuggerDisplay("{Serialize(false, 8).Trim()}")]
public class Line
{
    private readonly List<LineElement> _elements = [];

    /// <summary>Ordered list of elements that make up this line.</summary>
    public IReadOnlyList<LineElement> Elements => _elements;


    /// <summary>key = value</summary>
    public static Line KeyValue(
        string key, string value,
        Padding keyPadding   = default,
        Padding valuePadding = default)
    {
        var line = new Line();
        line.Add(new KeyElement(key, keyPadding) );
        line.Add(new ValueElement(value, valuePadding));
        
        return line;
    }

    public static Line KeyValues(
        string key,
        IEnumerable<string> values,
        Padding keyPadding = default,
        Padding valuesPadding = default)
    {
        var line = new Line();
        var items = values.Select(v => new ValueItem(v));
        line.Add(new KeyElement(key, keyPadding));
        line.Add(new ValueElement(items, valuesPadding));

        return line;
    }

    /// <summary>key = value ; comment</summary>
    public static Line KeyValueComment(
        string key, string value, string comment,
        Padding keyPadding     = default,
        Padding valuePadding   = default,
        Padding commentPadding = default)
    {
        var line = KeyValue(key, value, keyPadding, valuePadding);
        line.Add(new CommentElement(comment) { Padding = commentPadding });
        
        return line;
    }

    /// <summary>Return an empty line (serializes to an empty line).</summary>
    public static Line Empty() => new();

    /// <summary>Return a line that contains only a comment.</summary>
    public static Line Comment(string text, Padding padding = default)
    {
        var line = new Line();
        line.Add(new CommentElement(text, padding));

        return line;
    }

    /// <summary>Return an include directive line (#include "path").</summary>
    public static Line Include(string path, Padding padding = default)
    {
        var line = new Line();
        line.Add(new IncludeElement(path, padding));

        return line;
    }

    public void Add(LineElement element)
    {
        ArgumentNullException.ThrowIfNull(element);

        // Enforce: exactly one KeyElement per line
        if (element is KeyElement && _elements.Any(e => e is KeyElement))
            throw new InvalidOperationException("Line already contains a KeyElement.");

        // Enforce: exactly one ValueElement per line
        if (element is ValueElement && _elements.Any(e => e is ValueElement))
            throw new InvalidOperationException("Line already contains a ValueElement.");

        // Enforce: exactly one IncludeElement per line
        if (element is IncludeElement && _elements.Any(e => e is IncludeElement))
            throw new InvalidOperationException("Line already contains an IncludeElement.");

        // IncludeElement cannot coexist with KeyElement/ValueElement
        if (element is IncludeElement && _elements.Any(e => e is KeyElement or ValueElement))
            throw new InvalidOperationException("IncludeElement cannot coexist with KeyElement/ValueElement in the same Line.");

        if (element is (KeyElement or ValueElement) && _elements.Any(e => e is IncludeElement))
            throw new InvalidOperationException("KeyElement/ValueElement cannot coexist with IncludeElement in the same Line.");

        // CommentElement must be the last element in the line (or the only one)
        if (_elements.Any(e => e is CommentElement))
            throw new InvalidOperationException("CommentElement must be the only or the last element in the Line; no elements can follow it.");

        // Do not allow adding another CommentElement when one already exists as the last element
        if (element is CommentElement && _elements.Count > 0 && _elements[^1] is CommentElement)
            throw new InvalidOperationException("Line already contains a CommentElement.");

        _elements.Add(element);
    }

    /// <summary>Replace an element in the line while preserving its position in the elements list.</summary>
    public void Replace(LineElement oldElement, LineElement newElement)
    {
        var index = _elements.IndexOf(oldElement);
        if (index < 0)
            throw new InvalidOperationException("Element not found in this Line.");

        _elements[index] = newElement;
    }
    /// <summary>Remove an element from the line.</summary>
    /// <param name="element">Element to remove.</param>
    public void Remove(LineElement element) => _elements.Remove(element);

    /// <summary>Return the KeyElement if present, otherwise null.</summary>
    public KeyElement?     GetKey()     => _elements.OfType<KeyElement>().FirstOrDefault();

    /// <summary>Return the ValueElement if present, otherwise null.</summary>
    public ValueElement?   GetValue()   => _elements.OfType<ValueElement>().FirstOrDefault();

    /// <summary>Return value items if a ValueElement exists, otherwise null.</summary>
    public IEnumerable<ValueItem>? GetValues() => _elements.OfType<ValueElement>().FirstOrDefault()?.Items;

    /// <summary>Return the trailing comment element if present, otherwise null.</summary>
    public CommentElement? GetComment() => _elements.OfType<CommentElement>().FirstOrDefault();

    /// <summary>Return the include element if present, otherwise null.</summary>
    public IncludeElement? GetInclude() => _elements.OfType<IncludeElement>().FirstOrDefault();

    /// <summary>True when the line contains no elements.</summary>
    public bool IsEmpty    => _elements.Count == 0;

    /// <summary>True when the line contains a key/value pair.</summary>
    public bool IsKeyValue => GetKey() != null;

    /// <summary>True when the line is an include directive.</summary>
    public bool IsInclude  => GetInclude() != null;

    /// <summary>Serialize the line back to text. Optionally convert repeated spaces into tabs.</summary>
    /// <param name="spacesToTabs">When true, convert groups of spaces to tabs using tabSize.</param>
    /// <param name="tabSize">Tab width used when converting spaces to tabs.</param>
    public string Serialize(bool spacesToTabs = false, int tabSize = 8)
    {
        if (IsEmpty) return string.Empty;

        var sb = new StringBuilder();
        foreach (var el in _elements)
        {
            sb.Append(el.Serialize());
        }
        
        return spacesToTabs ? sb.ToString().SpacesToTabs(tabSize) : sb.ToString();
    }
}
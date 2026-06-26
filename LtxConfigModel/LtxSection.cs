using LtxConfigModel.LineElements;
using System.Diagnostics;
using System.Text;

namespace LtxConfigModel;

/// <summary>
/// Represents a section inside a .ltx document. A section may have a name, optional parents
/// (inheritance via [name]:parent1, parent2) and a sequence of lines that belong to the section.
/// The Padding property controls spaces inside the section header brackets.
/// </summary>
[DebuggerDisplay("{SerializeHeader()}")]
public class LtxSection
{
    private readonly List<Line>   _lines   = [];
    private readonly List<string> _parents = [];
    private LtxDocument? _document;

    /// <summary>Create a named section. Name must not be empty.</summary>
    /// <param name="name">Section name (non-empty).</param>
    /// <param name="padding">Padding inside the header brackets.</param>
    public LtxSection(string name, Padding padding = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name must be not empty.");

        Name = name;
        Padding = padding;
    }

    internal LtxSection()
    {
        Name = string.Empty;
    }

    internal void SetDocument(LtxDocument document)
    {
        _document = document;
    }

    /// <summary>True when this is the root (unnamed) section.</summary>
    public bool IsRoot => string.IsNullOrWhiteSpace(Name);

    /// <summary>Section name (empty for the root section).</summary>
    public string Name    { get; set; }

    /// <summary>Padding applied inside the section header brackets.</summary>
    public Padding Padding { get; set; } = Padding.None;

    /// <summary>Lines contained in this section.</summary>
    public IReadOnlyList<Line>   Lines   => _lines;

    /// <summary>Optional comment attached to the section header.</summary>
    public CommentElement? HeaderComment { get; set; }

    /// <summary>
    /// Return direct parent sections declared in the header ([name:parent1, parent2]).
    /// Each parent is resolved via the containing document (or created as a placeholder
    /// section when the document is not available or the parent is missing). The method is
    /// protected from cycles and will not return a section that already appeared in the chain.
    /// </summary>
    public IReadOnlyList<LtxSection> GetParents(HashSet<string>? visited = null)
    {
        visited ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (!visited.Add(Name))
            return [];

        var result = new List<LtxSection>(_parents.Count);

        foreach (var parentName in _parents)
        {
            if (visited.Contains(parentName))
                continue;

            var parent = ResolveParent(parentName);
            result.Add(parent);
        }

        return result;
    }

    /// <summary>
    /// Return the full inheritance tree of this section as a flat list in breadth-first order:
    /// direct parents first, then parents of those parents, and so on. The section itself is not included.
    /// The method avoids cycles and duplicates (each section appears at most once by name).
    /// </summary>
    public IReadOnlyList<LtxSection> GetAllParents()
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { Name };
        var result  = new List<LtxSection>();
        var queue   = new Queue<LtxSection>();

        foreach (var parentName in _parents)
        {
            if (visited.Add(parentName))
                queue.Enqueue(ResolveParent(parentName));
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var parentName in current._parents)
            {
                if (visited.Add(parentName))
                    queue.Enqueue(current.ResolveParent(parentName));
            }
        }

        return result;
    }
    // Lines manipulation

    /// <summary>Add a Line to this section.</summary>
    /// <param name="line">Line to add.</param>
    public Line Add(Line line)      { _lines.Add(line);    return line; }

    /// <summary>Remove a Line from this section.</summary>
    /// <param name="line">Line to remove.</param>
    public void Remove(Line line)   => _lines.Remove(line);

    /// <summary>Insert a Line at the specified index.</summary>
    /// <param name="i">Index at which to insert.</param>
    /// <param name="line">Line to insert.</param>
    public void Insert(int i, Line line) => _lines.Insert(i, line);

    /// <summary>Find the first line containing the specified key (case-insensitive).</summary>
    public Line? GetKeyValueLine(string key) => _lines
        .FirstOrDefault(l => l.IsKeyValue && string.Equals(l.GetKey()?.Key, key, StringComparison.OrdinalIgnoreCase));

    // Shortcuts
    /// <summary>Add a key=value line to the section.</summary>
    public Line AddKeyValue(string key, string value,
        Padding keyPadding = default, Padding valuePadding = default)
        => Add(Line.KeyValue(key, value, keyPadding, valuePadding));

    /// <summary>Add a key with multiple comma-separated values.</summary>
    public Line AddKeyValues(string key, IEnumerable<string> values, Padding keyPadding = default,
        Padding valuePadding = default)
        => Add(Line.KeyValues(key, values, keyPadding, valuePadding));

    /// <summary>Add a comment line to the section with optional left padding.</summary>
    public Line AddComment(string text, int leftPadding = 0)
        => Add(Line.Comment(text, new Padding(leftPadding, 0)));

    /// <summary>Add an empty line to the section.</summary>
    public Line AddEmpty()
        => Add(Line.Empty());

    /// <summary>Return all keys declared in this section.</summary>
    public string[] GetKeys() => _lines.Where(l => l.IsKeyValue).Select(l => l.GetKey()?.Key ?? string.Empty).ToArray();

    /// <summary>Return the single value for the given key, or null if missing.</summary>
    public string? GetValue(string key) => GetKeyValueLine(key)?.GetValue()?.SingleValue;

    /// <summary>Return all comma-separated values for the given key, or an empty array.</summary>
    public string[] GetValues(string key) => GetKeyValueLine(key)?.GetValue()?.Values.ToArray() ?? [];

    /// <summary>
    /// Replace the value of an existing key while preserving the key padding and the line comment.
    /// The new ValueElement inherits padding from the old ValueElement when present, otherwise Padding.None.
    /// Throws KeyNotFoundException if the key does not exist in this section.
    /// </summary>
    public void SetValue(string key, string value)
    {
        var line = GetKeyValueLine(key);

        if (line is null)
            throw new KeyNotFoundException($"Section '{Name}' does not contain key '{key}'.");

        var oldValue = line.GetValue();
        var valuePadding = oldValue?.Padding ?? Padding.None;

        var newValue = new ValueElement(value, valuePadding);

        if (oldValue is not null)
            line.Replace(oldValue, newValue);
        else
            line.Add(newValue);
    }

    /// <summary>
    /// Replace the list of values for an existing key while attempting to preserve each ValueItem's padding.
    /// If the new list is longer than the old one, additional items receive Padding.None. If shorter, excess
    /// old items are discarded. The outer ValueElement padding and the line comment are preserved.
    /// Throws KeyNotFoundException if the key is not present.
    /// </summary>
    /// <param name="key">Key whose values will be replaced.</param>
    /// <param name="values">New values to set.</param>
    public void SetValues(string key, IEnumerable<string> values)
    {
        var line = GetKeyValueLine(key);
        if (line is null)
            throw new KeyNotFoundException($"Section '{Name}' does not contain key '{key}'.");

        var oldValue = line.GetValue();
        var valuePadding = oldValue?.Padding ?? Padding.None;
        var oldItems = oldValue?.Items ?? [];

        var newValuesList = values.ToList();
        var newItems = new List<ValueItem>(newValuesList.Count);

        for (var i = 0; i < newValuesList.Count; i++)
        {
            var padding = i < oldItems.Count ? oldItems[i].Padding : Padding.None;
            newItems.Add(new ValueItem(newValuesList[i]) { Padding = padding });
        }

        var newValue = new ValueElement(newItems, valuePadding);

        if (oldValue is not null)
            line.Replace(oldValue, newValue);
        else
            line.Add(newValue);
    }
    /// <summary>Return true when the specified key exists in this section.</summary>
    public bool IsKeyExists(string key) => GetKeyValueLine(key) is not null;

    // Inheritance helpers
    /// <summary>Add a parent name to this section's header.</summary>
    public void AddParent(string parent)    => _parents.Add(parent);
    /// <summary>Remove a parent name from this section's header.</summary>
    public void RemoveParent(string parent) => _parents.Remove(parent);
    /// <summary>Add multiple parent names to this section.</summary>
    public void AddParents(IEnumerable<string> parents) => _parents.AddRange(parents);

    /// <summary>Serialize only the section header (including padding, parents and header comment if any).</summary>
    public string SerializeHeader()
    {
        if (string.IsNullOrWhiteSpace(Name))
            return string.Empty;

        var sb = new StringBuilder();
        sb.Append(Padding.LeftStr);
        sb.Append('[');
        sb.Append(Name);
        sb.Append(']');

        if (_parents.Count > 0)
        {
            sb.Append(':');
            sb.Append(string.Join(", ", _parents));
        }
        sb.Append(Padding.RightStr);

        if (HeaderComment is not null) 
            sb.Append(HeaderComment.Serialize());

        return sb.ToString();
    }

    /// <summary>Serialize the entire section (header + lines) back to text.</summary>
    public string Serialize(bool spacesToTabs = false, int tabSize = 8)
    {
        var sb = new StringBuilder();
        sb.AppendLine(SerializeHeader());
        foreach (var line in _lines)
        {
            sb.AppendLine(line.Serialize(spacesToTabs, tabSize));
        }
        return sb.ToString();
    }

    private LtxSection ResolveParent(string parentName)
    {
        var found = _document?.FindSection(parentName);
        return found ?? new LtxSection(parentName);
    }
}

using System.Text;

namespace LtxConfigModel;

/// <summary>
/// Represents a parsed .ltx document. Acts as a container for sections and provides
/// access to the root (unnamed) section via the Root property.
/// </summary>
public class LtxDocument
{
    private readonly List<LtxSection> _sections = [];

    /// <summary>Create an empty LTX document with a root (unnamed) section.</summary>
    public LtxDocument()
    {
        Root = new LtxSection();
        _sections.Add(Root);
    }

    /// <summary>Root (unnamed) section of the document.</summary>
    public LtxSection Root { get; }

    /// <summary>Paths referenced by #include directives that are present in the root section.</summary>
    public string[] Includes => Root.Lines
        .Where(l => l.IsInclude && !string.IsNullOrWhiteSpace(l.GetInclude()?.Path))
        .Select(l => l.GetInclude()!.Path)
        .ToArray();

    /// <summary>All sections in the document, root first.</summary>
    public IReadOnlyList<LtxSection> Sections => _sections;

    // Section management
    /// <summary>Create and add a new section with the given name.</summary>
    /// <param name="name">Section name.</param>
    public LtxSection AddSection(string name)
    {
        var section = new LtxSection(name);

        return AddSection(section);
    }

    /// <summary>Add an existing section instance to the document.</summary>
    /// <param name="section">Section to add.</param>
    public LtxSection AddSection(LtxSection section)
    {
        if (_sections.Any(s => s.Name.Equals(section.Name, StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException($"Section {section.Name} already exists.");

        section.SetDocument(this);
        _sections.Add(section);

        return section;
    }

    /// <summary>Remove a non-root section from the document.</summary>
    public void RemoveSection(LtxSection section)
    {
        if (section == Root)
            throw new InvalidOperationException("Cannot remove the root section.");

        _sections.Remove(section);
    }

    /// <summary>Find a section by name (case-insensitive). Returns null if not found.</summary>
    public LtxSection? FindSection(string name)
        => _sections.FirstOrDefault(s =>
            string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    // Shortcuts into the root section
    /// <summary>Add a raw line into the root (unnamed) section.</summary>
    public Line AddLine(Line line)       => Root.Add(line);

    /// <summary>Add an #include directive to the root section.</summary>
    /// <param name="path">Include path.</param>
    /// <param name="leftPadding">Left padding before the directive.</param>
    public Line AddInclude(string path, int leftPadding = 0)  => Root.Add(Line.Include(path, new Padding(leftPadding, 0)));

    /// <summary>Add a comment line to the root section.</summary>
    public Line AddComment(string text, int leftPadding = 0)  => Root.AddComment(text, leftPadding);

    /// <summary>Add an empty line to the root section.</summary>
    public Line AddEmpty()               => Root.AddEmpty();

    /// <summary>Serialize the document back to text. Optionally convert spaces to tabs.</summary>
    public string Serialize(bool spacesToTabs = false, int tabSize = 8)
    {
        var sb = new StringBuilder();

        // Root section has no header
        foreach (var line in Root.Lines)
            sb.AppendLine(line.Serialize(spacesToTabs, tabSize));

        // Other sections
        foreach (var section in _sections)
            sb.Append(section.Serialize(spacesToTabs, tabSize));

        return sb.ToString();
    }
}

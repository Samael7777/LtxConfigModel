using System.Text.RegularExpressions;
using LtxConfigModel.LineElements;

namespace LtxConfigModel;

/// <summary>
/// Simple parser for .ltx file content. Parses lines, section headers, include directives,
/// key/value pairs and preserves padding and comments in the resulting LtxDocument model.
/// Use LtxParser.Parse() to obtain an LtxDocument instance.
/// </summary>
public class LtxParser
{
    private static readonly Regex s_KeyValueRegex = new(@"^\s*([A-Za-z0-9_$.]+)\s*(?:=\s*(.*?))?\s*$");

    private readonly int _tabSize;
    private readonly LtxDocument _document;
    private readonly string[] _rawContent;

    private LtxSection _currentSection;
    private UnparsedLine? _currentUnparsedLine;

    /// <summary>
    /// Create a parser for the provided .ltx content.
    /// </summary>
    /// <param name="content">Full text content to parse.</param>
    /// <param name="tabSize">Tab width used to expand tabs into spaces during parsing.</param>
    public LtxParser(string content, int tabSize = 8)
    {
        _tabSize = tabSize;
        _document = new LtxDocument();
        _currentSection = _document.Root;
        _rawContent = content.Split(Environment.NewLine);
    }

    /// <summary>
    /// Parse the provided content and return a populated LtxDocument model.
    /// </summary>
    public LtxDocument Parse()
    {
        for (var i = 0; i < _rawContent.Length; i++)
        {
            var rawLine = _rawContent[i];
            _currentUnparsedLine = new UnparsedLine(rawLine.TabsToSpaces(_tabSize), i + 1);
            
            if (TryParseEmptyLine() 
                || TryParseCommentOnlyLine()
                || TryParseIncludeLine()
                || TryParseSectionHeader()
                || TryParseKeyValuePair())
            {
                continue;
            }
            
            throw new InvalidOperationException($"Line {i + 1} : incorrect format.");
        }

        return _document;
    }

    private bool TryParseEmptyLine()
    {
        if (_currentUnparsedLine?.DataElement is not null || _currentUnparsedLine?.CommentElement is not null) 
            return false;
        
        _currentSection.AddEmpty();
        return true;
    }

    private bool TryParseCommentOnlyLine()
    {
        if (_currentUnparsedLine?.DataElement is not null || _currentUnparsedLine?.CommentElement is null) 
            return false;

        _currentSection.AddComment(_currentUnparsedLine.CommentElement.Text, _currentUnparsedLine.CommentElement.Padding.Left);
        return true;

    }
    
    private bool TryParseIncludeLine()
    {
        if (_currentUnparsedLine?.DataElement is null)
            return false;

        var rawString = _currentUnparsedLine.DataElement.Text;

        if (!rawString.StartsWith("#include", StringComparison.OrdinalIgnoreCase))
            return false;

        var includePath = rawString[8..].Trim().Trim('"');
        var parsedLine = new Line();
        var includeElement = new IncludeElement(includePath, _currentUnparsedLine.DataElement.Padding);

        parsedLine.Add(includeElement);
        if (_currentUnparsedLine.CommentElement is not null)
            parsedLine.Add(_currentUnparsedLine.CommentElement);

        _document.AddLine(parsedLine);

        return true;
    }

    private bool TryParseSectionHeader()
    {
        if (_currentUnparsedLine?.DataElement is null)
            return false;

        var rawString = _currentUnparsedLine.DataElement.Text;
        if (string.IsNullOrWhiteSpace(rawString))
            return false;

        if (!rawString.StartsWith('['))
            return false;

        var endSectionNameIndex = rawString.IndexOf(']');
        if (endSectionNameIndex == -1)
            throw new InvalidOperationException($"Line {_currentUnparsedLine.RawLineNumber}: invalid data format.");

        var sectionName = rawString[1..endSectionNameIndex].Trim();
        var section = new LtxSection(sectionName, _currentUnparsedLine.DataElement.Padding);

        var parts = rawString.Split(':', 2);
        if (parts.Length == 2)
        {
            var parents = parts[1].Split(',')
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p));
            
            section.AddParents(parents);
        }

        section.HeaderComment = _currentUnparsedLine.CommentElement;
        _document.AddSection(section);
        _currentSection = section;

        return true;
    }

    private bool TryParseKeyValuePair()
    {
        if (_currentUnparsedLine?.DataElement is null)
            return false;

        var rawString = _currentUnparsedLine.DataElement.Text;
        var isKeyValueString = s_KeyValueRegex.Match(rawString);
        if (!isKeyValueString.Success)
            return false;

        var line = new Line();
        var key = isKeyValueString.Groups[1].Value;
        var keyLeftPadding = _currentUnparsedLine.DataElement.Padding.Left;
        var keyElem = new KeyElement(key.Trim(), new Padding(keyLeftPadding, key.RightPadding()));
        
        line.Add(keyElem);
        var isValues = isKeyValueString.Groups[2];
        if (isValues.Success)
        {
            var values = isValues.Value.Split(',');

            var items = values
                .Select(v => new ValueItem(v.Trim(), v.LeftPadding(), v.RightPadding()));

            var leftPadding = values[0].LeftPadding();
            var rightPadding = _currentUnparsedLine.DataElement.Padding.Right;
            line.Add(new ValueElement(items, new Padding(leftPadding, rightPadding)));
        }
        
        if (_currentUnparsedLine.CommentElement is not null)
            line.Add(_currentUnparsedLine.CommentElement);
        
        _currentSection.Add(line);
        
        return true;
    }

    
}
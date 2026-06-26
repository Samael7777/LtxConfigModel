LtxConfigModel
=================

A small .NET library for parsing, representing and serializing .ltx configuration files used by the S.T.A.L.K.E.R. series.

Features
- Parse .ltx text into a document model composed of sections and lines
- Preserve padding, comments and include directives
- Modify keys, values and sections programmatically and serialize back to text

Quick start

Install

This project targets .NET 10 and is a class library. Reference the compiled DLL or add the project to your solution.

Basic usage

```csharp
using LtxConfigModel;

var content = @"
[general]
fullscreen = true ; enable fullscreen
[render]
texture_detail = 2, 4, 6
";

// Parse
var parser = new LtxParser(content);
var doc = parser.Parse();

// Access sections
var render = doc.FindSection("render");
var keys = render?.GetKeys();
var textureValues = render?.GetValues("texture_detail");

// Modify a value
render?.SetValue("texture_detail", "3");

// Add new section and key
var sec = doc.AddSection("custom");
sec.AddKeyValue("enabled", "1");

// Serialize back to text
var output = doc.Serialize();
Console.WriteLine(output);
```

Working with lines and elements

The model exposes a Line class that contains a sequence of LineElement objects. Elements include:
- KeyElement — the left-hand key of a key/value line
- ValueElement — the value block (can contain multiple ValueItem tokens)
- CommentElement — trailing comment starting with ';'
- IncludeElement — `#include "path"` directive

You can create lines programmatically using factory helpers:

```csharp
var line = Line.KeyValue("my_key", "my_value");
var lineWithComment = Line.KeyValueComment("my_key", "my_value", "a note");
var include = Line.Include("configs\\other.ltx");
```

Preserving formatting

The library tries to preserve padding and comments when parsing and re-serializing. Use Padding values on elements when building lines manually to control spacing exactly.

Notes

This library focuses on faithful round-trip parsing and editing of .ltx files rather than interpreting semantics. It preserves whitespace and comments and allows safe programmatic editing of keys, values and sections.

License

See repository license (if any).
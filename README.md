LtxConfigModel
=================

A small .NET library for parsing, representing and serializing .ltx configuration files used by the S.T.A.L.K.E.R. series.

## Features

- Parse .ltx text into a document model composed of sections and lines
- Preserve padding, comments and include directives
- Modify keys, values and sections programmatically and serialize back to text

## Quick Start

### Installation

This project targets .NET 6, .NET 8, and .NET 10. Reference the NuGet package or add the project to your solution.

### Basic Usage

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

## Working with Lines and Elements

The model exposes a `Line` class that contains a sequence of `LineElement` objects. Elements include:

- **KeyElement** — the left-hand key of a key/value line
- **ValueElement** — the value block (can contain multiple ValueItem tokens)
- **CommentElement** — trailing comment starting with `;`
- **IncludeElement** — `#include "path"` directive

You can create lines programmatically using factory helpers:

```csharp
var line = Line.KeyValue("my_key", "my_value");
var lineWithComment = Line.KeyValueComment("my_key", "my_value", "a note");
var include = Line.Include("configs\\other.ltx");
```

## API Overview

### LtxDocument

Represents the entire parsed .ltx file:

- `Root` — the root (unnamed) section
- `Sections` — all sections in the document
- `AddSection(name)` — add a new section
- `FindSection(name)` — find a section by name (case-insensitive)
- `Serialize()` — convert back to text

### LtxSection

Represents a named section `[name:parent1, parent2]`:

- `Name` — section name
- `Lines` — list of lines in the section
- `Add(line)` — add a line to the section
- `AddKeyValue(key, value)` — shortcut to add a key/value pair
- `GetKeys()` — return all keys in the section
- `GetValue(key)` — get single value for a key
- `GetValues(key)` — get all comma-separated values for a key
- `SetValue(key, value)` — replace a value
- `SetValues(key, values)` — replace multiple values

### Line

Represents a single line of configuration:

- `KeyValue(key, value)` — factory for key/value lines
- `KeyValueComment(key, value, comment)` — factory with trailing comment
- `Include(path)` — factory for include directives
- `Comment(text)` — factory for comment-only lines
- `Empty()` — factory for empty lines
- `Add(element)` — add an element to the line
- `GetKey()` — get the KeyElement if present
- `GetValue()` — get the ValueElement if present
- `GetValues()` — get value items if present
- `Serialize()` — convert back to text

## Preserving Formatting

The library tries to preserve padding and comments when parsing and re-serializing. Use `Padding` values on elements when building lines manually to control spacing exactly.

```csharp
var padding = new Padding(left: 2, right: 1);  // 2 spaces left, 1 space right
var key = new KeyElement("my_key", padding);
```

## Notes

This library focuses on faithful round-trip parsing and editing of .ltx files rather than interpreting semantics. It preserves whitespace, comments, and include directives, allowing safe programmatic editing of keys, values, and sections.

## License

MIT

# Broccolini 🥦
Broccolini is a non-destructive parser for INI files.
The main goal is compatibility with the INI format used in Windows OS ([`GetPrivateProfileString`] and friends).

## Usage

<details>
<summary>INI file used in examples</summary>

```ini
[database]
server = 192.0.2.62
port = 1234
```

</details>

### Reading
```cs
var syntax = IniParser.Parse(File.ReadAllText("config.ini"));
var document = syntax.ToSemanticModel();
string databaseServer = document["database"]["server"];
string databasePort = document["database"]["port"];
```

## Editing
```cs
var syntax = IniParser.Parse(File.ReadAllText("config.ini"));
var updated = syntax
    .WithSection("owner", section => section.WithKeyValue("name", "John Doe"))
    .UpdateSection("database", section => section.RemoveKeyValue("port"));
File.WriteAllText("config.ini", updated.ToString(), Encoding.Unicode);
```

## Goals
* Compatibiliy with INI format in Windows OS.
* Roundtrips (`Parse` -> `ToString`) should preserve everything.
* Editing should preserve as much (whitespace, comments, etc.) as possible.
* An extensive test suite.

## Non-Goals
* Deserialization into non-string types (complex objects, numbers, booleans, etc.)
* Customizable parsing rules (e.g. different comment syntax)
* Performance


[`GetPrivateProfileString`]: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring

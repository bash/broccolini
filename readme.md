# Broccolini 🥦

[![Build](https://github.com/bash/broccolini/workflows/Build/badge.svg)](https://github.com/bash/broccolini/actions?query=workflow%3ABuild)
[![NuGet package](https://buildstats.info/nuget/Broccolini)](https://www.nuget.org/packages/Broccolini)

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
var document = IniParser.ParseToSemanticModel(File.ReadAllText("config.ini"));
string databaseServer = document["database"]["server"];
string databasePort = document["database"]["port"];
```

### Editing
```cs
var document = IniParser.Parse(File.ReadAllText("config.ini"));
var updated = document
    .WithSection("owner", section => section.WithKeyValue("name", "John Doe"))
    .UpdateSection("database", section => section.RemoveKeyValue("port"));
File.WriteAllText("config.ini", updated.ToString(), Encoding.Unicode);
```

## Stability
This library is feature-complete and stable.
Contributions are welcome, please create an issue first for discussion.

## Known Differences
While Broccolini tries to replicate most of the behaviour found in the Windows APIs,
there are still some intentional differences:

* `GetPrivateProfileString` does not support keys or section names that contain NULL characters (`\0`). \
   Broccolini supports such keys and section names.
* While the editing API in Broccolini shares the goal of changing as little as possible, its behaviour
  does not explicitly replicate the behaviour of `WritePrivateProfileString`.
* `GetPrivateProfileString` breaks when a unicode file contains a non-trailing NULL character (`\0`). \
   Broccolini supports such files.

## Goals
* Compatibiliy with INI format in Windows OS.
* Roundtrips (`Parse` → `ToString`) should preserve everything.
* Editing should preserve as much (whitespace, comments, etc.) as possible.
* An extensive test suite.

## Non-Goals
* Deserialization into non-string types (complex objects, numbers, booleans, etc.)
* Customizable parsing rules (e.g. different comment syntax)
* Performance


## License
Licensed under the MIT license ([license-mit.txt](license-mit.txt) or <http://opensource.org/licenses/MIT>).

## Contribution
Unless you explicitly state otherwise, any contribution intentionally submitted
for inclusion in the work by you, as defined in the [Apache-2.0 license](http://www.apache.org/licenses/LICENSE-2.0), shall be
licensed as above, without any additional terms or conditions.

[`GetPrivateProfileString`]: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring

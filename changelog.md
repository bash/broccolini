# Changelog
## 1.0.0
This release polishes the API by simplifying names, namespaces and adding some convenience:
Most notably, all important types now have a `Ini` prefix: `IniDocument`, `IIniSection`, etc.
All of the APIs which are expected to be used by regular consumers of the library now live in the main namespace `Broccolini`.

The main entry point of the library is as before the `IniParser` class with two methods:
* `Parse` which parses the document into an AST that preserves formatting and comments for editing.
* `ParseToSemanticModel` which parses the document into a semantic representation optimized for reading.

A lot of advanced APIs are now marked as `[EditorBrowsable(Advanced)]` to create a pit of success
when looking at the API surface.

## 0.2.2
* Improve insertion heuristic for keys when section contains trailing whitespace. (#5)

## 0.2.1
* Fix broken newline detection for appending a node to new section (Follow up to #1).

## 0.2.0
* Fix incorrect package description.
* Fix broken newline detection for appending a node to an empty section. (#1)
* Use stronger types in AST.
* Distinguish comment nodes from unrecognized nodes in AST.
* Treat all characters `< ' '` as whitespace to align with Windows' behaviour.

## 0.1.0
Initial release

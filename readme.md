# Broccolini 🥦
Broccolini is a non-destructive parser for INI files.
The main goal is compatibility with the INI parsing present in Windows OS ([`GetPrivateProfileString`] and friends).

## Goals
* Compatibiliy with INI parsing in Windows OS.
* Roundtrips (`Parse` -> `ToString`) should preserve everything.
* Editing should preserve as much (whitespace, comments, etc.) as possible.
* An extensive test suite.

## Non-Goals
* Deserialization into non-string types (complex objects, numbers, booleans, etc.)
* Performance


[`GetPrivateProfileString`]: https://docs.microsoft.com/en-us/windows/win32/api/winbase/nf-winbase-getprivateprofilestring

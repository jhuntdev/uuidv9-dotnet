# UUID v9

## Fast, lightweight, zero-dependency .NET implementation of UUID version 9

The v9 UUID supports both sequential (time-based) and non-sequential (random) UUIDs with an optional prefix of up to four bytes, an optional checksum, and sufficient randomness to avoid collisions. It uses the UNIX timestamp for sequential UUIDs and CRC-8 for checksums. A version digit can be added if desired, but is omitted by default.

To learn more about UUID v9, please visit the website: https://uuidv9.jhunt.dev

## Installation

Install UUID v9 from NuGet

```bash
dotnet add package UUIDv9
```

## Usage

```c#
using System;
using UUIDv9;

class Program
{
  static void Main()
  {
    var orderedId = UUIDv9();
    var prefixedOrderedId = UUIDv9(prefix: "a1b2c3d4");
    var unorderedId = UUIDv9(timestamp: false);
    var prefixedUnorderedId = UUIDv9(prefix: "a1b2c3d4", timestamp: false);
    var orderedIdWithChecksum = UUIDv9(checksum: true);
    var orderedIdWithVersion = UUIDv9(version: true);
    var orderedIdWithLegacyMode = UUIDv9(legacy: true);

    var isValid = IsValidUuidV9(orderedId);
    var isValidWithChecksum = IsValidUuidV9(orderedIdWithChecksum, checksum: true);
    var isValidWithVersion = IsValidUuidV9(orderedIdWithVersion, version: true);
  }
}

```

## Backward Compatibility

Some UUID validators check for specific features of v1 or v4 UUIDs. This causes some valid v9 UUIDs to appear invalid. Three possible workarounds are:

1) Use the built-in validator (recommended)
2) Use legacy mode*
3) Bypass the validator (not recommended)

_*Legacy mode adds version and variant digits to immitate v1 or v4 UUIDs depending on the presence of a timestamp._

## License

This project is licensed under the [MIT License](LICENSE).
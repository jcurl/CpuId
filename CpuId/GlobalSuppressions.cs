﻿// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "P/Invoke", Scope = "namespaceanddescendants", Target = "N:RJCP.Diagnostics.Native")]
[assembly: SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "P/Invoke", Scope = "namespaceanddescendants", Target = "N:RJCP.Diagnostics.Native")]
[assembly: SuppressMessage("Minor Code Smell", "S2344:Enumeration type names should not have \"Flags\" or \"Enum\" suffixes", Justification = "P/Invoke", Scope = "namespaceanddescendants", Target = "N:RJCP.Diagnostics.Native")]

[assembly: SuppressMessage("Critical Code Smell", "S3218:Inner class members should not shadow outer class \"static\" or type members", Justification = "Static Classes", Scope = "type", Target = "T:RJCP.Diagnostics.CpuId.Intel.AmdBrandIdentifier")]

[assembly: SuppressMessage("Major Code Smell", "S4070:Non-flags enums should not be marked with \"FlagsAttribute\"", Justification = "Flags are legitimate")]

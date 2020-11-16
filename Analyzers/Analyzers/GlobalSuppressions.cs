// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

// Most warnings about shipping are because we are not planning to ship
// this analyzer, it is exclusively for contributors of the project.

[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.StaticFieldThreadSafety.RuleShouldBeNull")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.StaticFieldThreadSafety.RuleAddAttribute")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.EitherAbstractOrSealed.Rule")]

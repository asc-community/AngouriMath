//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using System.Diagnostics.CodeAnalysis;

// Most warnings about shipping are because we are not planning to ship
// this analyzer, it is exclusively for contributors of the project.

[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.StaticFieldThreadSafety.RuleShouldBeNull")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.StaticFieldThreadSafety.RuleAddAttribute")]
[assembly: SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "<Pending>", Scope = "member", Target = "~F:Analyzers.EitherAbstractOrSealed.Rule")]

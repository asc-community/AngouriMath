//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldThreadSafetyCodeFixProvider)), Shared]
    public class FieldThreadSafetyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(EitherAbstractOrSealed.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

#pragma warning disable CS1998 // To be implemented yet
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
#pragma warning restore CS1998
        {
            
        }
        
    }
}

//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;


namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EitherAbstractOrSealed : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "SealedOrAbstract";
        private static readonly string Title = "AMAnalyzer";
        private static readonly string MessageFormat = $"If a type is not sealed, it should be either static or abstract";
        private static readonly string Description = "Unsafe type extension prevention.";
        private const string Category = "Security";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(symbolContext =>
            {
                var typeDecl = (INamedTypeSymbol)symbolContext.Symbol;
                var containingType = typeDecl.ContainingType;
                if (!typeDecl.IsAbstract && !typeDecl.IsSealed && !typeDecl.IsStatic)
                {
                    var diag = Diagnostic.Create(Rule, typeDecl.Locations.First());
                    symbolContext.ReportDiagnostic(diag);
                }
            }, SymbolKind.NamedType);
        }
    }
}

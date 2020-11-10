using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;


namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NonStaticFieldWithoutEquals : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "PrivateFieldsDanger";
        private static readonly string Title = "AMAnalyzer";
        private static readonly string MessageFormat = $"There should be no fields inside records unless {nameof(object.Equals)} is overriden";
        private static readonly string Description = "Stack overflow prevention";
        private const string Category = "Security";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        private static bool IsRecord(TypeKind tk)
            => tk == TypeKind.Unknown;

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(symbolContext =>
            {
                var typeDecl = (IFieldSymbol)symbolContext.Symbol;
                var containingType = typeDecl.ContainingType;

                if (!typeDecl.IsStatic &&
                    IsRecord(containingType.TypeKind) &&
                    !containingType.MemberNames.Contains("Equals"))
                {
                    var diag = Diagnostic.Create(Rule, typeDecl.Locations.First());
                    symbolContext.ReportDiagnostic(diag);
                }
            }, SymbolKind.Field);
        }
    }
}

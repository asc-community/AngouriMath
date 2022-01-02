//
// Copyright (c) 2019-2022 Angouri.
// AngouriMath is licensed under MIT.
// Details: https://github.com/asc-community/AngouriMath/blob/master/LICENSE.md.
// Website: https://am.angouri.org.
//

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticFieldThreadSafety : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ThreadSafety";

        private static readonly DiagnosticDescriptor RuleAddAttribute = 
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "AMAnalyzer", 
                messageFormat: "A static field should have either [ConstantField] attribute or [ThreadStatic] or [ConcurrentField]",
                category: "Security",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "Data corruption prevention.");

        private static readonly DiagnosticDescriptor RuleShouldBeNull =
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "AMAnalyzer",
                messageFormat: "A thread static field should have no initialization (instead, should have an additional nullable backing field)",
                category: "Security",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "Non-initialized field prevention.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(RuleAddAttribute); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(symbolContext =>
            {
                var fieldDecl = (IFieldSymbol)symbolContext.Symbol;
                var hasConstantFieldAttribute = fieldDecl.GetAttributes().Any(attr => attr.AttributeClass?.Name == "ConstantFieldAttribute");
                var hasThreadStaticAttribute = fieldDecl.GetAttributes().Any(attr => attr.AttributeClass?.Name == "ThreadStaticAttribute");
                var hasConcurrentFieldAttribute = fieldDecl.GetAttributes().Any(attr => attr.AttributeClass?.Name == "ConcurrentFieldAttribute");
                if (fieldDecl.IsStatic && 
                    !fieldDecl.IsConst && 
                    !hasThreadStaticAttribute
                    &&
                    !hasConstantFieldAttribute
                    &&
                    !hasConcurrentFieldAttribute
                    &&
                    fieldDecl.Type.Name != "ConditionalWeakTable"
                    )
                {
                    var diag = Diagnostic.Create(RuleAddAttribute, fieldDecl.Locations.First());
                    symbolContext.ReportDiagnostic(diag);
                }
                if (hasThreadStaticAttribute)
                {
                    var syntaxTree = fieldDecl.DeclaringSyntaxReferences.First().GetSyntax();
                    if (syntaxTree is VariableDeclaratorSyntax variableDeclarator)
                    {
                        if (variableDeclarator.Initializer is not null)
                        {
                            var diag = Diagnostic.Create(RuleShouldBeNull, fieldDecl.Locations.First());
#pragma warning disable RS1005 // ReportDiagnostic invoked with an unsupported DiagnosticDescriptor
                            symbolContext.ReportDiagnostic(diag);
#pragma warning restore RS1005 // ReportDiagnostic invoked with an unsupported DiagnosticDescriptor
                        }
                    }
                }
            }, SymbolKind.Field);
        }
    }
}

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
    public class StaticFieldThreadSafety : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ThreadSafety";

        private static readonly DiagnosticDescriptor RuleAddAttribute = 
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "AMAnalyzer", 
                messageFormat: "A static field should have either [ConstantField] attribute or [ThreadStatic] (for cache)",
                category: "Security",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true,
                description: "Data corruption prevention.");

        private static readonly DiagnosticDescriptor RuleShouldBeNull =
            new DiagnosticDescriptor(
                id: DiagnosticId,
                title: "AMAnalyzer",
                messageFormat: "A thread static field should have no initialization (instead, should have an additional nullable backing field) {0}",
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
                if (fieldDecl.IsStatic && 
                    !fieldDecl.IsConst && 
                    !hasThreadStaticAttribute
                    &&
                    !hasConstantFieldAttribute
                    &&
                    fieldDecl.Type.Name != "ConditionalWeakTable"
                    )
                {
                    var diag = Diagnostic.Create(RuleAddAttribute, fieldDecl.Locations.First());
                    symbolContext.ReportDiagnostic(diag);
                }
                if (hasThreadStaticAttribute)
                {
                    var diag = Diagnostic.Create(RuleShouldBeNull, fieldDecl.Locations.First(),
                         string.Join(", ", fieldDecl.DeclaringSyntaxReferences.Select(c => c.SyntaxTree.GetType())));
                    symbolContext.ReportDiagnostic(diag);
                }
            }, SymbolKind.Field);
        }
    }
}

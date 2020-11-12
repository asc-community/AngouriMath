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
        private static readonly string Title = "AMAnalyzer";
        private static readonly string MessageFormat = "A static field should have either [ConstantField] attribute or [ThreadStatic] (for cache)";
        private static readonly string Description = "Data corruption prevention.";
        private const string Category = "Security";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSymbolAction(symbolContext =>
            {
                var fieldDecl = (IFieldSymbol)symbolContext.Symbol;
                if (fieldDecl.IsStatic && 
                    !fieldDecl.IsConst && 
                    !fieldDecl.GetAttributes().Any(attr => attr.AttributeClass?.Name == "ConstantFieldAttribute")
                    &&
                    !fieldDecl.GetAttributes().Any(attr => attr.AttributeClass?.Name == "ThreadStaticAttribute")
                    &&
                    fieldDecl.Type.Name != "ConditionalWeakTable"
                    )
                {
                    var diag = Diagnostic.Create(Rule, fieldDecl.Locations.First(), 
                        $"Only {string.Join(", ", fieldDecl.GetAttributes().Select(c => c.AttributeClass?.Name ?? ""))} present");
                    symbolContext.ReportDiagnostic(diag);
                }
            }, SymbolKind.Field);
        }
    }
}

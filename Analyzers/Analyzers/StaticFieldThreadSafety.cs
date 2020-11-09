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
        public const string DiagnosticId = "Analyzers";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/Localizing%20Analyzers.md for more on localization
        //private static readonly LocalizableString Title = new LocalizableResourceString("AMAnalyzer", Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString MessageFormat = new LocalizableResourceString("", Resources.ResourceManager, typeof(Resources));
        //private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private static readonly string Title = "AMAnalyzer";
        private static readonly string MessageFormat = "A static field should have either [ConstantField] attribute or [ThreadStatic] (for cache)";
        private static readonly string Description = "";
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxTreeAction(syntaxTreeContext =>
            {
                var root = syntaxTreeContext.Tree.GetRoot(syntaxTreeContext.CancellationToken);
                foreach (var fieldDeclaration in root.DescendantNodes().OfType<FieldDeclarationSyntax>())
                {
                    var isStatic = fieldDeclaration.Modifiers.Any(c => c.Kind() == SyntaxKind.StaticKeyword);

                    var hasThreadStaticAlready = fieldDeclaration
                                .AttributeLists
                                .Any(list => list.Attributes.Any(c => c.ToFullString() == "ThreadStatic"));

                    var hasConstantFieldAlready = fieldDeclaration
                                .AttributeLists
                                .Any(list => list.Attributes.Any(c => c.ToFullString() == "ConstantField"));

                    var isConditionalWeakTable = fieldDeclaration.Declaration.Type.GetFirstToken().Text == "ConditionalWeakTable";

                    if (isStatic && !hasThreadStaticAlready && !hasConstantFieldAlready && !isConditionalWeakTable)
                    {
                        //var tokenVariable = fieldDeclaration.ChildTokens().Where(c => c.Kind() == SyntaxKind.IdentifierName).First();
                        var tokenVariable = fieldDeclaration.Declaration.Variables.First();
                        //var tokenVariable = fieldDeclaration.GetFirstToken();
                        var diagnostic = Diagnostic.Create(Rule, tokenVariable.GetLocation());
                        syntaxTreeContext.ReportDiagnostic(diagnostic);
                    }
                }
            });
        }

#pragma warning disable IDE0051 // Remove unused private members (we might need it in the future)
        private static void AnalyzeSymbol(SymbolAnalysisContext _)
#pragma warning restore IDE0051 // Remove unused private members
        {
            
        }
    }
}

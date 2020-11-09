using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldThreadSafetyCodeFixProvider)), Shared]
    public class FieldThreadSafetyCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(StaticFieldThreadSafety.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.Where(d => d.Id == "ThreadSafety").FirstOrDefault();

            if (diagnostic is null)
                return;

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent;

            if (declaration is null)
                return;

            if (declaration is not FieldDeclarationSyntax fieldDecl)
                return;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: "Add [ConstantField] attribute",
                    createChangedSolution: c => AddConstantFieldAttribute(context.Document, fieldDecl, c),
                    equivalenceKey: "what is this"
                    ),
                    diagnostic);
        }

        private async Task<Solution> AddConstantFieldAttribute(Document document, FieldDeclarationSyntax fieldDecl, CancellationToken cancellationToken)
        {
            var nameSyntax = SyntaxFactory.IdentifierName("ConstantField");
            var attribute = SyntaxFactory.Attribute(nameSyntax);
            var attributeList = SyntaxFactory.AttributeList(new SeparatedSyntaxList<AttributeSyntax>().Add(attribute));
            var newDecl = fieldDecl.AddAttributeLists(attributeList);

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(fieldDecl, cancellationToken);
            var fieldD = (IFieldSymbol)typeSymbol;

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}

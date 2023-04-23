using System.Collections.Immutable;
using System.Text;
using DreamBig.Repository.Cosmos.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DreamBig.Repository.Cosmos.Generators;

[Generator]
public sealed class RepoGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "UseFastRepoAttribute.g.cs",
            SourceText.From("", Encoding.UTF8)
            ));

        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations
            = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemantricTargetForGeneration(ctx)
                )
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationsAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationsAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc)
            );
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemantricTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        foreach (AttributeListSyntax listSyntax in classDeclarationSyntax.AttributeLists)
        {
            foreach (AttributeSyntax syntax in listSyntax.Attributes)
            {
                if (context.SemanticModel.GetSymbolInfo(syntax).Symbol is not IMethodSymbol attributeSymbol)
                {
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();
                if (fullName == "DreamBig.Repository.Cosmos.Attributes.UseFastRepoAttribute")
                {
                    return classDeclarationSyntax;
                }
            }
        }
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty)
        {
            return;
        }

        IEnumerable<ClassDeclarationSyntax> distinctClasses = classes.Distinct();
        List<ClassToGenerate> classesToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);
        //throw new Exception(System.Text.Json.JsonSerializer.Serialize(classesToGenerate));
        if (classesToGenerate.Count > 0)
        {
            string results = "";
            context.AddSource("", SourceText.From(results, Encoding.UTF8));
        }
    }

    private static List<ClassToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken cancellationToken)
    {
        List<ClassToGenerate> classesToGenerate = new();
        INamedTypeSymbol? classAttribute = compilation.GetTypeByMetadataName("DreamBig.Repository.Cosmos.Attributes.UseFastRRepoAttribute");
        if (classAttribute is null)
        {
            return classesToGenerate;
        }

        foreach (ClassDeclarationSyntax syntax in classes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SemanticModel semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(syntax) is not INamedTypeSymbol classSymbol)
            {
                continue;
            }

            string className = classSymbol.Name;
            string classNamespace = classSymbol.ContainingNamespace.Name;
            classesToGenerate.Add(new(classNamespace, className));
        }
        return classesToGenerate;
    }
}

using DreamBig.Repository.Cosmos.Generators.Models;
using DreamBig.Repository.Exceptions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Text;

namespace DreamBig.Repository.Cosmos.Generators;

[Generator]
public sealed class RepoGenerator : IIncrementalGenerator
{
    private const string AttributeResourceName = "DreamBig.Repository.Cosmos.Generators.Templates.UseRepoAttribute.cs";
    private const string RepoResourceName = "DreamBig.Repository.Cosmos.Generators.Templates._TheEntity_Repository.cs";
    private const string AttributeFullQualifiedName = "DreamBig.Repository.Cosmos.Attributes.UseRepoAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        GenerateAttribute(context);

        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations
            = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx)
                )
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationsAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilationsAndClasses,
            static (spc, source) => Execute(source.Item1, source.Item2, spc)
            );
    }

    private static void GenerateAttribute(IncrementalGeneratorInitializationContext context)
    {
        using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(AttributeResourceName)
            ?? throw new RepositoryException("Embedded resource not found");
        using StreamReader reader = new(stream);
        var attributeCodes = reader.ReadToEnd();

        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
            "UseRepoAttribute.g.cs",
            SourceText.From(attributeCodes, Encoding.UTF8)
            ));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
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
                if (fullName == AttributeFullQualifiedName)
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
        if (classesToGenerate.Count > 0)
        {
            using Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(RepoResourceName)
                ?? throw new RepositoryException("Embedded resource not found");
            using StreamReader reader = new(stream);
            var repositoryTemplateCodes = reader.ReadToEnd();

            foreach (var classToGenerate in classesToGenerate)
            {
                var repositoryCodes = repositoryTemplateCodes.Replace("_TheEntity_", classToGenerate.ClassName)
                    .Replace("_TheNamespace_", classToGenerate.NamespaceName);
                string generatedClassName = $"{classToGenerate.ClassName}Repository.g.cs";
                context.AddSource(generatedClassName, SourceText.From(repositoryCodes, Encoding.UTF8));
            }
        }
    }

    private static List<ClassToGenerate> GetTypesToGenerate(Compilation compilation, IEnumerable<ClassDeclarationSyntax> classes, CancellationToken cancellationToken)
    {
        List<ClassToGenerate> classesToGenerate = new();
        INamedTypeSymbol? classAttribute = compilation.GetTypeByMetadataName(AttributeFullQualifiedName);
        if (classAttribute is null)
        {
            return classesToGenerate;
        }

        foreach (ClassDeclarationSyntax syntax in classes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            SemanticModel semanticModel = compilation.GetSemanticModel(syntax.SyntaxTree);
            if (semanticModel.GetDeclaredSymbol(syntax, cancellationToken: cancellationToken) is not INamedTypeSymbol classSymbol)
            {
                continue;
            }

            string className = classSymbol.Name;
            string classNamespace = classSymbol.ContainingNamespace.ToDisplayString();
            classesToGenerate.Add(new() { ClassName = className, NamespaceName = classNamespace });
        }
        return classesToGenerate;
    }
}

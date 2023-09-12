using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Diagnostics;

namespace Metadata.Design;

[Generator]
public class MetadataSourceGenerator : IIncrementalGenerator
{
    private const string ObjectConfigurerAttributeFullName = "Metadata.Core.MetaModels.Attributes.ObjectConfigurerAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Do a simple filter for enums
        IncrementalValuesProvider<GenerateContext> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<GenerateContext>)> compilationAndEnums
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // context.RegisterPostInitializationOutput(x => {
        //     x.AddSource(
        //         "ObjectConfigurerAttribute.g.cs",
        //         AssetManager.ReadFileAsString("ObjectConfigurerAttribute.cs"));
        // });

        // Generate the source using the compilation and enums
        context.RegisterSourceOutput(compilationAndEnums,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax m && m.AttributeLists.Count > 0;
    }

    private static GenerateContext? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var declarationSyntax = (ClassDeclarationSyntax)context.Node;

        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in declarationSyntax.AttributeLists) {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes) {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                // Is the attribute the [EnumExtensions] attribute?
                if (fullName == ObjectConfigurerAttributeFullName) {
                    var a = (attributeSyntax.ArgumentList!.Arguments[0].Expression as TypeOfExpressionSyntax).Type as IdentifierNameSyntax;
                    // return the enum
                    return new GenerateContext(declarationSyntax, a);
                }
            }
        }

        // we didn't find the attribute we were looking for
        return null;
    }

    private static void Execute(Compilation compilation, ImmutableArray<GenerateContext> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) {
            // nothing to do yet
            return;
        }

        // I'm not sure if this is actually necessary, but `[LoggerMessage]` does it, so seems like a good idea!
        // TODO
        IEnumerable<GenerateContext> distinctClasses = classes.Distinct();

        foreach (var generateContext in distinctClasses) {
            generateContext.WithCompilation(compilation);

            var configurerClassIdentifier = generateContext.ClassDeclarationSyntax.Identifier;
            var semanticModel = compilation.GetSemanticModel(generateContext.ClassDeclarationSyntax.SyntaxTree);
            var entityTypeSymbol = semanticModel.GetSymbolInfo(generateContext.IdentifierName).Symbol;

            generateContext.SemanticModel = semanticModel;

            GeneratePartialConfigurer(generateContext, context);
            GeneratePartialBo(generateContext, context);

            var b = entityTypeSymbol.GetType();
            var entityFullName = entityTypeSymbol.ToString();
            int pos = entityFullName.LastIndexOf('.');
            string ns = string.Empty;
            string entityName;

            if (pos > 0) {
                ns = entityFullName.Substring(0, pos);
                entityName = entityFullName.Substring(pos + 1);
            } else {
                entityName = entityFullName;
            }

            string source = $@"public partial class {entityName}ObjectMeta2 {{
}}";

            if (!string.IsNullOrEmpty(ns)) {
                source = $@"namespace {ns} {{
{source}
}}";
            }

            context.AddSource($"Metadata_{entityName}.g.cs", source);

            // foreach (var attributeListSyntax in declaration.AttributeLists) {
            //     foreach (var attributeSyntax in attributeListSyntax.Attributes) {
            //         if (semanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) {
            //             // weird, we couldn't get the symbol, ignore it
            //             continue;
            //         }
            //     }
            // }
        }
        // // Convert each EnumDeclarationSyntax to an EnumToGenerate
        // List<EnumToGenerate> enumsToGenerate = GetTypesToGenerate(compilation, distinctClasses, context.CancellationToken);
        // 
        // // If there were errors in the EnumDeclarationSyntax, we won't create an
        // // EnumToGenerate for it, so make sure we have something to generate
        // if (enumsToGenerate.Count > 0) {
        //     // generate the source code and add it to the output
        //     string result = SourceGenerationHelper.GenerateExtensionClass(enumsToGenerate);
        //     context.AddSource("EnumExtensions.g.cs", SourceText.From(result, Encoding.UTF8));
        // }
    }

    private static void GeneratePartialConfigurer(GenerateContext generateContext, SourceProductionContext context)
    {
        var modifiers = generateContext.ClassDeclarationSyntax.Modifiers;

        string accessModifier = string.Empty;
        string staticModifier = string.Empty;

        foreach (var modifier in modifiers) {
            if (modifier.IsKind(SyntaxKind.PublicKeyword)) {
                accessModifier = "public";
            } else if (modifier.IsKind(SyntaxKind.ProtectedKeyword)) {
                accessModifier = "protected";
            } else if (modifier.IsKind(SyntaxKind.PrivateKeyword)) {
                accessModifier = "private";
            } else if (modifier.IsKind(SyntaxKind.StaticKeyword)) {
                staticModifier = "static";
            }
        }
        var declaredSymbol = generateContext.SemanticModel!.GetDeclaredSymbol(generateContext.ClassDeclarationSyntax) as ITypeSymbol;

        string source = $@"{accessModifier} {staticModifier} partial class {generateContext.ConfigurerClassName}
{{
    public static PropertyMeta<TValue> Property<TValue>(Expression<Func<{generateContext.ClassName}, TValue>> propertyExpr)
    {{
        return new PropertyMeta<TValue>(""Name"");
    }}
}}";

        if (declaredSymbol.ContainingNamespace is not null) {
            source = $@"namespace {generateContext.ConfigurerNamespace}
{{
    {source}
}}";
        }

        source = $@"using Metadata.Core.MetaModels.Attributes;
using System.Linq.Expressions;
using MetaModels.Entities;
using {generateContext.FullNamespaceName};

{source}";

        context.AddSource($"{generateContext.ConfigurerClassName}.g.cs", source);
    }

    private static void GeneratePartialBo(GenerateContext generateContext, SourceProductionContext context)
    {
        var boClassName = $"{generateContext.ClassName}Bo";

        var members = generateContext.ClassDeclarationSyntax.Members;
    }
}

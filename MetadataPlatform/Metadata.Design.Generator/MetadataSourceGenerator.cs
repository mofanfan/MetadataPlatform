using Metadata.Design.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Text;

namespace Metadata.Design;

[Generator]
public class MetadataSourceGenerator : IIncrementalGenerator
{
    private const string ObjectConfigurerClassFullName = "MetaModels.Entities.ObjectConfigurer<>";
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
        return node is ClassDeclarationSyntax /* m && m.AttributeLists.Count > 0 */;
    }

    private static GenerateContext? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var declarationSyntax = (ClassDeclarationSyntax)context.Node;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax);
        if (classSymbol == null) {
            return null;
        }

        var baseType = classSymbol.FindBaseType("MetaModels.Entities.ObjectConfigurer<>");
        if (baseType == null) {
            return null;
        }

        var members = classSymbol.GetMembers();

        var entityTypeSymbol = baseType.TypeArguments.First();

        return new GenerateContext(declarationSyntax, classSymbol, null, entityTypeSymbol);

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
                    return new GenerateContext(declarationSyntax, classSymbol, a, entityTypeSymbol);
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

            // GeneratePartialConfigurer(generateContext, context);
            GeneratePartialBo(generateContext, context);

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
        var members = generateContext.ConfigurerTypeSymbol.GetMembers();

        StringBuilder sourceBuilder = new();
        var boClassName = $"{generateContext.Entity!.ClassName}Bo";

        sourceBuilder.AppendLine($"namespace {generateContext.ConfigurerNamespace}");
        sourceBuilder.AppendLine("{");
        sourceBuilder.AppendLine($"    public class {boClassName}");
        sourceBuilder.AppendLine("    {");

        foreach (var member in members) {
            if (member is not IFieldSymbol fieldSymbol) {
                continue;
            }

            var syntaxReference = fieldSymbol.DeclaringSyntaxReferences.Single();
            var variableDeclaratorSyntax = syntaxReference.GetSyntax() as VariableDeclaratorSyntax;

            if (variableDeclaratorSyntax == null) {
                continue;
            }

            if (variableDeclaratorSyntax.Initializer == null) {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        new DiagnosticDescriptor("ME001", "title", "message", "category", DiagnosticSeverity.Error, true),
                        variableDeclaratorSyntax.GetLocation()));
                return;
            }

            if (variableDeclaratorSyntax.Initializer.Value is InvocationExpressionSyntax invocationExpressionSyntax) {
                var methodSymbol = generateContext.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
                if (methodSymbol == null) {
                    throw new ArgumentNullException(nameof(methodSymbol));
                }

                if (methodSymbol.ReturnType is not INamedTypeSymbol returnType) {
                    throw new Exception();
                }

                var baseTypeSymbol = returnType.FindBaseType("MetaModels.Entities.PropertyMetadata<>");
                if (baseTypeSymbol == null) {
                    continue;
                }

                var valueTypeSymbol = baseTypeSymbol.TypeArguments.First();
                var valueTypeFullName = valueTypeSymbol.ToDisplayString();

                sourceBuilder.AppendLine($"        public {valueTypeFullName} {fieldSymbol.Name} {{ get; set; }}");
            }

            var symbolInfo = generateContext.SemanticModel.GetSymbolInfo(variableDeclaratorSyntax.Initializer.Value);
            var a = generateContext.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax.Initializer.Value);
            var b = generateContext.SemanticModel.GetTypeInfo(variableDeclaratorSyntax.Initializer.Value);
        }

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");

        var sourceText = sourceBuilder.ToString();
        context.AddSource($"{boClassName}.g.cs", sourceText);

        // var members = generateContext.ClassDeclarationSyntax.Members;
    }
}

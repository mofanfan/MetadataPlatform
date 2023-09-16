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
    private const string ObjectMetadataRegisterClassFullName = "MetaModels.Entities.ObjectMetadataRegister<>";
    private const string ObjectConfigurerClassFullName = "MetaModels.Entities.ObjectConfigurer<>";
    private const string ObjectConfigurerAttributeFullName = "Metadata.Core.MetaModels.Attributes.ObjectConfigurerAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        IncrementalValuesProvider<GenerateContext> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        IncrementalValueProvider<(Compilation, ImmutableArray<GenerateContext>)> compilations
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        context.RegisterSourceOutput(compilations,
            static (spc, source) => Execute(source.Item1, source.Item2, spc));

        context.RegisterImplementationSourceOutput(
            compilations,
            static (spc, source) => ExecuteModuleInitializer(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
    {
        return node is ClassDeclarationSyntax m
            && m.BaseList is not null
            /*&& m.AttributeLists.Count > 0*/;
    }

    private static GenerateContext? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var declarationSyntax = (ClassDeclarationSyntax)context.Node;

        var classSymbol = context.SemanticModel.GetDeclaredSymbol(declarationSyntax);
        if (classSymbol == null) {
            return null;
        }

#if true
        var baseType = classSymbol.FindBaseType(ObjectMetadataRegisterClassFullName);
        if (baseType == null) {
            return null;
        }

        var members = classSymbol.GetMembers();

        var entityTypeSymbol = baseType.TypeArguments.First();

        return new GenerateContext(declarationSyntax, classSymbol, null, entityTypeSymbol);
#else
        // loop through all the attributes on the method
        foreach (AttributeListSyntax attributeListSyntax in declarationSyntax.AttributeLists) {
            foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes) {
                if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol attributeSymbol) {
                    // weird, we couldn't get the symbol, ignore it
                    continue;
                }

                INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
                string fullName = attributeContainingTypeSymbol.ToDisplayString();

                if (fullName == ObjectConfigurerAttributeFullName) {
                    var argument0 = attributeSyntax.ArgumentList!.Arguments[0];
                    if (argument0.Expression is not TypeOfExpressionSyntax typeOfExpression) {
                        // TODO: 分析器提示参数必需为typeof()表达式
                        return null;
                    }

                    if (typeOfExpression.Type is not IdentifierNameSyntax entityIdentifierName) {
                        var message = $"expression is not an identifier syntax, {typeOfExpression.Type.GetLocation()}";
                        throw new ArgumentException(message);
                    }

                    var entityTypeSymbolInfo = context.SemanticModel.GetSymbolInfo(entityIdentifierName);
                    var entitySymbol = entityTypeSymbolInfo.Symbol as ITypeSymbol;
                    return new GenerateContext(declarationSyntax, classSymbol, null, entitySymbol!);
                }
            }
        }
#endif

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
            if (context.CancellationToken.IsCancellationRequested) {
                return;
            }

            generateContext.WithCompilation(compilation);

            GenerateMetadata(generateContext, context);
            GeneratePartialMetadataRegister(generateContext, context);
            // GeneratePartialBo(generateContext, context);
        }
    }

    private static void GenerateMetadata(GenerateContext generateContext, SourceProductionContext context)
    {
        var configurerInfo = generateContext.Configurer;
        if (configurerInfo == null) {
            throw new ArgumentNullException(nameof(configurerInfo));
        }

        var entityInfo = generateContext.Entity;
        if (entityInfo == null) {
            throw new ArgumentNullException(nameof(entityInfo));
        }

        var configurerName = configurerInfo.ClassName;
        var metadataClassName = $"{entityInfo.ClassName}Metadata";
        var metadataNamespace = configurerInfo.NamespaceName;
        var namespaceIntent = string.IsNullOrEmpty(metadataNamespace) ? string.Empty : "    ";

        var sourceBuilder = new StringBuilder();

        SourceTextHelper.WriteAutoGeneratedHeader(sourceBuilder);

        sourceBuilder.AppendLine("using MetaModels.Entities;");
        sourceBuilder.AppendLine();

        if (!string.IsNullOrEmpty(metadataNamespace)) {
            sourceBuilder.AppendLine($"namespace {metadataNamespace}");
            sourceBuilder.AppendLine("{");
        }

        sourceBuilder.AppendLine($"{namespaceIntent}public sealed class {metadataClassName}");
        sourceBuilder.AppendLine($"{namespaceIntent}    : ObjectMetadata");
        sourceBuilder.AppendLine($"{namespaceIntent}{{");

        var configurerMembers = generateContext.ConfigurerTypeSymbol.GetMembers();

        foreach (var member in configurerMembers) {
            if (!member.IsStatic) {
                continue;
            }
            if (member is not IFieldSymbol field) {
                continue;
            }
            if (!field.IsReadOnly) {
                continue;
            }

            var fieldType = field.Type as INamedTypeSymbol;
            if (fieldType == null) {
                throw new ArgumentNullException(nameof(fieldType));
            }
            if (!fieldType.IsGenericType) {
                continue;
            }

            var fieldTypeName = fieldType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            // TODO: public?
            // TODO: based PropertyMetadata<> ?

            sourceBuilder.AppendLine($"{namespaceIntent}    public {fieldTypeName} {field.Name} => {configurerName}.{field.Name};");
        }

        sourceBuilder.AppendLine($"{namespaceIntent}}}");

        if (!string.IsNullOrEmpty(metadataNamespace)) {
            sourceBuilder.AppendLine("}");
        }

        var sourceText = sourceBuilder.ToString();

        context.AddSource($"{metadataClassName}.g.cs", sourceText);
    }

    private static void GeneratePartialMetadataRegister(GenerateContext generateContext, SourceProductionContext context)
    {
        var entityInfo = generateContext.Entity;
        if (entityInfo == null) {
            throw new ArgumentNullException(nameof(entityInfo));
        }

        var configurerInfo = generateContext.Configurer!;
        var modifiers = generateContext.ClassDeclarationSyntax.Modifiers;

        string accessModifier = string.Empty;

        foreach (var modifier in modifiers) {
            if (modifier.IsKind(SyntaxKind.PublicKeyword)) {
                accessModifier = "public";
            } else if (modifier.IsKind(SyntaxKind.ProtectedKeyword)) {
                accessModifier = "protected";
            } else if (modifier.IsKind(SyntaxKind.PrivateKeyword)) {
                accessModifier = "private";
            }
        }

        var sourceBuilder = new StringBuilder();
        var className = configurerInfo.ClassName;
        var namespaceName = configurerInfo.NamespaceName;
        var hasNamespaceIntent = string.IsNullOrEmpty(namespaceName) ? string.Empty : "    ";

        var location = generateContext.ClassDeclarationSyntax.GetLocation();
        sourceBuilder.AppendLine($@"//------------------------------------------------------------------------------
// <auto-generated>
// File: {location.SourceTree?.FilePath}
// {location.GetLineSpan().Span.Start}
// </auto-generated>
//----------------------");

        sourceBuilder.AppendLine("using MetaModels.Entities;");
        sourceBuilder.AppendLine($"using {entityInfo.NamespaceName};");
        sourceBuilder.AppendLine();

        if (!string.IsNullOrEmpty(namespaceName)) {
            sourceBuilder.AppendLine($"namespace {namespaceName}");
            sourceBuilder.AppendLine("{");
        }

        sourceBuilder.AppendLine($"{hasNamespaceIntent}{accessModifier} partial class {className}");
        sourceBuilder.AppendLine($"{hasNamespaceIntent}    : IObjectMetadataConfigurer<{entityInfo.ClassName}Metadata>");
        sourceBuilder.AppendLine($"{hasNamespaceIntent}{{");
        sourceBuilder.AppendLine($"{hasNamespaceIntent}}}");

        if (!string.IsNullOrEmpty(namespaceName)) {
            sourceBuilder.AppendLine("}");
        }

        context.AddSource($"{className}.g.cs", sourceBuilder.ToString());
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

    private static void ExecuteModuleInitializer(Compilation compilation, ImmutableArray<GenerateContext> generateContexts, SourceProductionContext context)
    {
        var builder = new StringBuilder();

        builder.AppendLine("using MetaModels.Options;");
        builder.AppendLine();
        builder.AppendLine("namespace Metadata.Core");
        builder.AppendLine("{");
        builder.AppendLine("    internal static class MetadataModule");
        builder.AppendLine("    {");
        builder.AppendLine("        public static void ConfigureOptions(ObjectMetadataOptions options)");
        builder.AppendLine("        {");

        foreach (var gc in generateContexts) {
            var metadataNamespace = gc.Configurer!.NamespaceName;
            var configurerClassFullName = gc.Configurer!.FullName;
            var metadataClassName = $"{gc.Entity!.ClassName}Metadata";
            var metadataClassFullName = string.IsNullOrEmpty(metadataClassName) ? metadataClassName : $"{metadataNamespace}.{metadataClassName}";
            builder.AppendLine($"           options.RegisterIfNotContains<global::{configurerClassFullName}, global::{metadataClassFullName}>();");
        }

        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("Metadata.Core.MetadataModule.g.cs", builder.ToString());
    }
}

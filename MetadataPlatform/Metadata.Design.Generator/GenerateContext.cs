using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metadata.Design;

internal class ConfigurerContext
{
    public string FullName { get; }
    public string NamespaceName { get; }
    public string ClassName { get; }

    public ConfigurerContext(string fullName, string namespaceName, string className)
    {
        FullName = fullName;
        NamespaceName = namespaceName;
        ClassName = className;
    }
}

internal class EntityContext
{
    public string FullName { get; }
    public string NamespaceName { get; }
    public string ClassName { get; }

    public EntityContext(string fullName, string namespaceName, string className)
    {
        FullName = fullName;
        NamespaceName = namespaceName;
        ClassName = className;
    }
}

internal class GenerateContext
{
    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
    public INamedTypeSymbol ConfigurerTypeSymbol { get; }
    public ITypeSymbol EntityTypeSymbol { get; }
    public ConfigurerContext? Configurer { get; private set; }
    public EntityContext? Entity { get; private set; }
    public IdentifierNameSyntax IdentifierName { get; }
    public SemanticModel SemanticModel { get; set; }

    public string ConfigurerNamespace { get; set; }
    public string ConfigurerClassName { get; private set; }

    public GenerateContext(
        ClassDeclarationSyntax classDeclarationSyntax,
        INamedTypeSymbol configurerTypeSymbol,
        IdentifierNameSyntax identifierNameSyntax,
        ITypeSymbol entityTypeSymbol)
    {
        ClassDeclarationSyntax = classDeclarationSyntax;
        ConfigurerTypeSymbol = configurerTypeSymbol;
        IdentifierName = identifierNameSyntax;
        EntityTypeSymbol = entityTypeSymbol;
    }

    public void WithCompilation(Compilation compilation)
    {
        SemanticModel = compilation.GetSemanticModel(ClassDeclarationSyntax.SyntaxTree);

        InitializeEntityContext();
        ExtractConfigurerIdentifiers();
    }

    private void InitializeEntityContext()
    {
        var fullName = EntityTypeSymbol.ToString();
        (var namespaceName, var className) = ExtractClassName(fullName);

        Entity = new EntityContext(fullName, namespaceName, className);
    }

    private void ExtractConfigurerIdentifiers()
    {
        var fullName = ConfigurerTypeSymbol.ToString();
        (var namespaceName, var className) = ExtractClassName(fullName);
        Configurer = new ConfigurerContext(fullName, namespaceName, className);
    }

    private static (string, string) ExtractClassName(string fullClassName)
    {
        string ns = string.Empty;
        string cls;

        int pos = fullClassName.LastIndexOf('.');

        if (pos > 0) {
            ns = fullClassName.Substring(0, pos);
            cls = fullClassName.Substring(pos + 1);
        } else {
            cls = fullClassName;
        }

        return (ns, cls);
    }
}

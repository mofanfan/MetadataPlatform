using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metadata.Design;

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
    public EntityContext? Entity { get; private set; }
    public IdentifierNameSyntax IdentifierName { get; }
    public SemanticModel SemanticModel { get; set; }
    public string FullNamespaceName { get; set; }
    public string FullClassName { get; set; }
    public string ClassName { get; set; }

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
        string className, namespaceName = string.Empty;
        int pos = fullName.LastIndexOf('.');

        if (pos > 0) {
            namespaceName = fullName.Substring(0, pos);
            className = fullName.Substring(pos + 1);
        } else {
            className = fullName;
        }

        Entity = new EntityContext(fullName, namespaceName, className);
    }

    private void ExtractConfigurerIdentifiers()
    {
        var configurerSymbol = SemanticModel.GetDeclaredSymbol(ClassDeclarationSyntax) as ITypeSymbol ?? throw new Exception("无法获取Configurer符号");

        (ConfigurerNamespace, ConfigurerClassName) = ExtractClassName(configurerSymbol.ToString());
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

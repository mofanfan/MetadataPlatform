using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Metadata.Design;

internal class GenerateContext
{
    public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
    public IdentifierNameSyntax IdentifierName { get; }
    public SemanticModel SemanticModel { get; set; }
    public string FullNamespaceName { get; set; }
    public string FullClassName { get; set; }
    public string ClassName { get; set; }

    public string ConfigurerNamespace { get; set; }
    public string ConfigurerClassName { get; private set; }

    public GenerateContext(ClassDeclarationSyntax classDeclarationSyntax, IdentifierNameSyntax identifierNameSyntax)
    {
        ClassDeclarationSyntax = classDeclarationSyntax;
        IdentifierName = identifierNameSyntax;
    }

    public void WithCompilation(Compilation compilation)
    {
        SemanticModel = compilation.GetSemanticModel(ClassDeclarationSyntax.SyntaxTree);

        ExtractIdentifiers();
        ExtractConfigurerIdentifiers();
    }

    private void ExtractIdentifiers()
    {
        var entityTypeSymbol = SemanticModel.GetSymbolInfo(IdentifierName).Symbol ?? throw new Exception("无法获取实体符号");

        FullClassName = entityTypeSymbol.ToString();
        int pos = FullClassName.LastIndexOf('.');

        if (pos > 0) {
            FullNamespaceName = FullClassName.Substring(0, pos);
            ClassName = FullClassName.Substring(pos + 1);
        } else {
            ClassName = FullClassName;
        }
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

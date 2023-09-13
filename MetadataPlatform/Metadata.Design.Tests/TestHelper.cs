using Metadata.Core.MetaModels.Attributes;
using Metadata.Demo.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq.Expressions;
using System.Reflection;

namespace Metadata.Design.Tests;

internal static class TestHelper
{
    public static Task Verify(string source)
    {
        // Parse the provided string into a C# syntax tree
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(source);

        IEnumerable<PortableExecutableReference> references = new[] {
            MetadataReference.CreateFromFile(Assembly.Load(new AssemblyName("System.Runtime")).Location),
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(ObjectConfigurerAttribute).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Material).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Expression<>).Assembly.Location),
        };
        // Create a Roslyn compilation for the syntax tree.
        CSharpCompilation compilation = CSharpCompilation.Create(
            assemblyName: "Tests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new MetadataSourceGenerator();

        // The GeneratorDriver is used to run our generator against a compilation
        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);

        var diagnostics = compilation.GetDiagnostics()
            .Where(x => x.Severity == DiagnosticSeverity.Error);

        Assert.Empty(diagnostics);

        // Run the source generator!
        driver = driver.RunGenerators(compilation);

        // Use verify to snapshot test the source generator output!
        return Verifier.Verify(driver);
    }
}

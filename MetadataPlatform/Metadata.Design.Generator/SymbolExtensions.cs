using Microsoft.CodeAnalysis;

namespace Metadata.Design.Generator;

internal static class SymbolExtensions
{
    public static INamedTypeSymbol? FindBaseType(this INamedTypeSymbol? namedTypeSymbol, string baseTypeFullName)
    {
        if (namedTypeSymbol == null) {
            return null;
        }

        if (namedTypeSymbol.IsGenericType) {
            var tempSymbol = namedTypeSymbol.ConstructUnboundGenericType();
            if (tempSymbol.ToDisplayString() == baseTypeFullName) {
                return namedTypeSymbol;
            }
        } else if (namedTypeSymbol.ToDisplayString() == baseTypeFullName) {
            return namedTypeSymbol;
        }

        return FindBaseType(namedTypeSymbol.BaseType, baseTypeFullName);
    }
}

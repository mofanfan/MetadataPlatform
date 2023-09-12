using System.Reflection;

namespace Metadata.Design;

internal static class AssetManager
{
    public static string ReadFileAsString(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        resourceName = $"Metadata.Design.Assets.{resourceName}";

        var a = assembly.GetManifestResourceNames();

        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }
}

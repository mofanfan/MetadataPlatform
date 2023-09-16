//HintName: Metadata.Core.MetadataModule.g.cs
using MetaModels.Options;

namespace Metadata.Core
{
    internal static class MetadataModule
    {
        public static void ConfigureOptions(ObjectMetadataOptions options)
        {
           options.RegisterIfNotContains<global::Test.MaterialConfigurer, global::Test.MaterialMetadata>();
        }
    }
}

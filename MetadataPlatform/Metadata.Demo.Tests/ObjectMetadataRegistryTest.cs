using Metadata.Core;
using MetaModels.Entities;
using MetaModels.Options;
using Xunit;

namespace Metadata.Demo.Tests;

public class ObjectMetadataRegistryTest
{
    [Fact]
    public void Test_Build()
    {
        var options = new ObjectMetadataOptions();
        MetadataModule.ConfigureOptions(options);
        var registry = new ObjectMetadataRegistry(options);
        registry.Build();
    }
}

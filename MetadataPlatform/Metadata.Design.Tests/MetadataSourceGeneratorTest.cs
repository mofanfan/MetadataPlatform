namespace Metadata.Design.Tests;

[UsesVerify]
public class MetadataSourceGeneratorTest
{
    [Fact]
    public Task Test()
    {
        var source = @"using MetaModels.Entities;
using System;

namespace Test
{

    public class Material
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public Guid UnitId { get; set; }
    }

    public partial class MaterialConfigurer
        : ObjectConfigurer<Material>
    {
        public readonly PropertyMetadata Code =
            P<Material>.Property(x => x.Code, x => {
                x.WithRequired(true);
            });

        public readonly PropertyMetadata Name =
            P<Material>.Property(x => x.Name);

        public readonly PropertyMetadata MasterUnit =
            P<Material>.Property(x => x.UnitId);

        // TODO: TEST public readonly PropertyMetadata Test;
    }
}";

        return TestHelper.Verify(source);
    }
}

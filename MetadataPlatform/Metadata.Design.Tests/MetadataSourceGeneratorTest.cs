namespace Metadata.Design.Tests;

[UsesVerify]
public class MetadataSourceGeneratorTest
{
    [Fact]
    public Task Test()
    {
        var source = @"using MetaModels.Entities;
using Metadata.Core.MetaModels.Attributes;
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
        : ObjectMetadataRegister<Material>
    {
        public static readonly PropertyMetadata<string> Code =
            P<Material>.Property(x => x.Code);

        public static readonly PropertyMetadata<string> Name =
            P<Material>.Property(x => x.Name);

        public static readonly PropertyMetadata<Guid> MasterUnit =
            P<Material>.Property(x => x.UnitId);

        // TODO: TEST public readonly PropertyMetadata Test;
    }
}";

        return TestHelper.Verify(source);
    }
}

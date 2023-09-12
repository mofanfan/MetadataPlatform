namespace Metadata.Design.Tests;

[UsesVerify]
public class MetadataSourceGeneratorTest
{
    [Fact]
    public Task Test()
    {
        var source = @"using System;
using Metadata.Core.MetaModels.Attributes;

namespace Test
{
public class UnitEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

[ObjectConfigurer(typeof(UnitEntity))]
public static partial class UnitObjectConfigurer
{
}
}";

        return TestHelper.Verify(source);
    }
}

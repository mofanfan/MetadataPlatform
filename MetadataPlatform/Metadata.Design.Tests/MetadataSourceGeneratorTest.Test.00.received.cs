//HintName: UnitEntity.g.cs
namespace Test
{
    public static partial class UnitObjectConfigurer
{
    public static PropertyMeta Property<TValue>(Expression<Func<MaterialEntity, TValue>> propertyExpr)
    {
        return new PropertyMeta<string>("Name");
    }
}
}
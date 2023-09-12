using Metadata.Core.MetaModels.Attributes;
using MetaModels.Entities;
using System.Linq.Expressions;

namespace Metadata.Demo.Entities;

public class UnitBo
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

[ObjectConfigurer(typeof(Material))]
public static partial class MaterialConfigurer
{
    public static readonly PropertyMeta Code =
        Property(x => x.Code)
            .HasLabel("编码");

    public static readonly PropertyMeta UnitName =
        Property(x => x.UnitId, ReadUnit)
            .HasLabel("主单位");


    public static PropertyMeta<TReadValue> Property<TValue, TReadValue>(
        Expression<Func<Material, TValue>> propertyExpr,
        Func<TValue, TReadValue> getter)
    {
        return new PropertyMeta<TReadValue>("Name");
    }

    private static UnitBo ReadUnit(Guid id)
    {
        return null;
    }
}

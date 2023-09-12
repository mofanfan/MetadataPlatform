using System.Linq.Expressions;

namespace MetaModels.Entities;

public class UnitEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class MaterialEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public Guid UnitId { get; set; }
}

////////////////////////////////////////////////////////////////////////////////
/// 对象配置

public class ObjectConfigurer<TEntity>
{
    public PropertyMeta Property<TValue>(Expression<Func<TEntity, TValue>> propertyExpr)
    {
        throw new NotImplementedException();
    }
}

[ObjectConfigurer(typeof(UnitEntity))]
public static partial class UnitObjectConfigurer
{
    public static PropertyMeta Name => Property(x => x.Name);

    public static PropertyMeta Property<TValue>(Expression<Func<UnitEntity, TValue>> propertyExpr)
    {
        return new PropertyMeta<string>("Name");
    }
}

[ObjectConfigurer(typeof(MaterialEntity))]
public static partial class MaterialConfigurer
{
    public static PropertyMeta Code =>
        Property(x => x.Code)
            .HasLabel("编码");

    public static PropertyMeta Name => Property(x => x.Name);

    public static PropertyMeta Property<TValue>(Expression<Func<MaterialEntity, TValue>> propertyExpr)
    {
        return new PropertyMeta<string>("Name");
    }
}

////////////////////////////////////////////////////////////////////////////////
/// 对象元信息

public class UnitObjectMeta : ObjectMeta
{
    public override Type EntityType => typeof(UnitEntity);
    public override IEnumerable<Type> EntityTypes => new Type[] { typeof(UnitEntity) };
    public PropertyMeta Name { get; } = new PropertyMeta<string>("Name");
}

public class MaterialObjectMeta : ObjectMeta
{
    public override Type EntityType => typeof(MaterialEntity);
    public override IEnumerable<Type> EntityTypes => new Type[] { typeof(MaterialEntity) };
    public PropertyMeta Name { get; } = new PropertyMeta<string>("Name");
    public PropertyMeta Code { get; } = new PropertyMeta<Guid>("Code");
    public PropertyMeta Unit { get; } = new PropertyMeta<UnitBo>("Unit");
}

////////////////////////////////////////////////////////////////////////////////

public class UnitBo
{
    public string Name { get; set; }
}

public class MaterialBo
{
    public string Code { get; set; }
    public string Name { get; set; }
    public UnitBo Unit { get; set; }
}
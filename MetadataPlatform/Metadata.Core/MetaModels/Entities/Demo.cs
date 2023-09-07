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
/// 对象元信息

public class UnitObjectMeta : ObjectMeta
{
    public override Type RuntimeType => typeof(MaterialEntity);
    public PropertyMeta Name { get; } = new PropertyMeta<string>("Name");
}

public class MaterialObjectMeta : ObjectMeta
{
    public override Type RuntimeType => typeof(MaterialEntity);
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
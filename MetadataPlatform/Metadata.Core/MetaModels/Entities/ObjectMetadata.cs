namespace MetaModels.Entities;

public abstract class ObjectMetadata
{
    private readonly Type _entityType;

    internal protected ObjectMetadata(Type entityType)
    {
        _entityType = entityType;
    }

    public Type GetEntityType()
    {
        return _entityType;
    }
}

public abstract class ObjectMetadata<T>
    : ObjectMetadata
{
    protected ObjectMetadata()
        : base(typeof(T))
    {
    }
}

public abstract class ObjectMetadataRef
    : IObjectMetadataElement
{
    public Type TargetType { get; }

    /// <summary>
    /// !!!LAZY BINDING!!!
    /// </summary>
    public ObjectMetadata DeclaringMetadata { get; private set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    internal protected ObjectMetadataRef(Type targetType)
    {
        TargetType = targetType;
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal protected void ConfigureInternal(ObjectMetadata declaringMetadata)
    {
        DeclaringMetadata = declaringMetadata;
    }
}

public class ObjectMetadataRef<T>
    : ObjectMetadataRef
    where T : ObjectMetadata
{
    public T Target { get; private set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    public ObjectMetadataRef()
        : base(typeof(T))
    {
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal void ConfigureInternal(ObjectMetadata declaringMetadata, T target)
    {
        Target = target;
        ConfigureInternal(declaringMetadata);
    }
}

namespace MetaModels.Entities;

public abstract class ObjectMetadataRef
    : IObjectMetadataElement
{
    public Type TargetType { get; }

    /// <summary>
    /// Lazy binding
    /// </summary>
    public ObjectMetadata DeclaringMetadata { get; private set; }

    /// <summary>
    /// Lazy binding
    /// </summary>
    public ObjectMetadata Target { get; private set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
    internal protected ObjectMetadataRef(Type targetType)
    {
        TargetType = targetType;
    }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

    internal void ConfigureInternal(ObjectMetadata declaringMetadata, ObjectMetadata target)
    {
        DeclaringMetadata = declaringMetadata;
        Target = target;
    }
}

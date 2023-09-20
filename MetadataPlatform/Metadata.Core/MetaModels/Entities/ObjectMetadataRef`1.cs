namespace MetaModels.Entities;

public class ObjectMetadataRef<T>
    : ObjectMetadataRef
    where T : ObjectMetadata
{
    public new T Target => (T)base.Target;

    public ObjectMetadataRef()
        : base(typeof(T))
    {
    }
}

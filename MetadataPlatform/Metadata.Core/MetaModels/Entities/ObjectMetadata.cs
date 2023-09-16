namespace MetaModels.Entities;

public abstract class ObjectMetadata
{
}

public class ObjectMetadataRef<T>
    where T : ObjectMetadata
{
    public T Target { get; private set; }

    public ObjectMetadataRef()
    {
    }
}

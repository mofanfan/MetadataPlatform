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

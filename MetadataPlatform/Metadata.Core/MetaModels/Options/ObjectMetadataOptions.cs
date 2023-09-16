using MetaModels.Entities;

namespace MetaModels.Options;

public class ObjectMetadataOptions
{
    public IReadOnlyCollection<Type> ObjectMetadataTypes => _objectMetadataTypeMap.Values;
    internal IReadOnlyDictionary<Type, Type> ObjectMetadataTypeMap => _objectMetadataTypeMap;
    private readonly Dictionary<Type, Type> _objectMetadataTypeMap = new();

    public ObjectMetadataOptions RegisterIfNotContains<TRegister, TMetadata>()
        where TRegister : class, new()
        where TMetadata : ObjectMetadata
    {
        _objectMetadataTypeMap.TryAdd(typeof(TRegister), typeof(TMetadata));
        return this;
    }
}

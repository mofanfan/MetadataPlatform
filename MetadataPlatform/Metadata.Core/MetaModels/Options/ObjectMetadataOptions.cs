using MetaModels.Entities;

namespace MetaModels.Options;

public class ObjectMetadataOptions
{
    public IReadOnlyCollection<Type> ObjectMetadataTypes => _objectMetadataTypeMap.Values;
    internal IReadOnlyDictionary<Type, Type> ObjectMetadataTypeMap => _objectMetadataTypeMap;
    internal IReadOnlyDictionary<Type, Type> ObjectMetadataTypeMap2 => _objectMetadataTypeMap2;
    private readonly Dictionary<Type, Type> _objectMetadataTypeMap = new();
    private readonly Dictionary<Type, Type> _objectMetadataTypeMap2 = new();

    public ObjectMetadataOptions RegisterIfNotContains<TRegister, TMetadata>()
        where TRegister : class, new()
        where TMetadata : ObjectMetadata
    {
        _objectMetadataTypeMap.TryAdd(typeof(TRegister), typeof(TMetadata));
        _objectMetadataTypeMap2.TryAdd(typeof(TMetadata), typeof(TRegister));
        return this;
    }
}

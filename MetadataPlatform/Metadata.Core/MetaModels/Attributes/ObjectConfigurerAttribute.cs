namespace Metadata.Core.MetaModels.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ObjectConfigurerAttribute : Attribute
{
    public ObjectConfigurerAttribute(Type entityType)
    {
    }
}

public sealed class ObjectMetadata : Attribute
{
    public ObjectMetadata(Type declarationType)
    {
    }
}
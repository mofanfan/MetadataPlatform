namespace Metadata.Design;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ObjectConfigurerAttribute : Attribute
{
    public ObjectConfigurerAttribute(Type entityType)
    {
    }
}

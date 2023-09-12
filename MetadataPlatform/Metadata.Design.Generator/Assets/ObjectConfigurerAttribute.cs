namespace Metadata.Design
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal sealed class ObjectConfigurerAttribute : Attribute
    {
        public ObjectConfigurerAttribute(Type entityType)
        {
        }
    }
}

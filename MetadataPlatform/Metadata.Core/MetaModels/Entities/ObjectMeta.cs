using System.Reflection;

namespace MetaModels.Entities;

public abstract class ObjectMeta
{
    public abstract Type RuntimeType { get; }
}

public abstract class PropertyMeta
{
    public abstract Type RuntimeType { get; }
    public abstract string Name { get; }
}

public class PropertyMeta<T> : PropertyMeta
{
    public override Type RuntimeType => typeof(T);
    public override string Name { get; }

    public PropertyMeta(string name)
    {
        Name = name;
    }
}
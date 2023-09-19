using MetaModels.Reflaction;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaModels.Entities;

public abstract class PropertyMetadata
{
    public ObjectMetadata DeclaringMetadata { get; private set; }
    public string Name { get; protected set; }
    public string Label { get; internal set; }
    public Type Type { get; protected set; }

    /// <summary>
    /// 必填
    /// </summary>
    public bool Required { get; internal set; }

    public IReadOnlyDictionary<string, object?> Annotations => _annotations;
    public MaintenanceOwners MaintenanceOwners { get; internal set; }
    public MaintenanceKind MaintenanceKind { get; internal set; }
    public bool PartOfCoding { get; internal set; }

    /// <summary>
    /// 必填关联项
    /// </summary>
    public IEnumerable<PropertyMetadata> Requires => _requires;

    private readonly Dictionary<string, object?> _annotations = new();
    private readonly List<PropertyMetadata> _requires = new();

    internal PropertyMetadata(PropertyInfo propertyInfo)
    {
        Name = propertyInfo.Name;
        Type = propertyInfo.PropertyType;
        Label = Name;
    }

    internal PropertyMetadata(string name, Type type)
    {
        Name = name;
        Type = type;
        Label = Name;
    }

    internal void SetAnnonation(string name, object? value)
    {
        _annotations[name] = value;
    }

    internal void RemoveAnnonation(string name)
    {
        _annotations.Remove(name);
    }

    internal void WithRequired(bool b)
    {
        Required = b;
    }

    internal void ConfigureInternal(ObjectMetadata declaringMetadata)
    {
        DeclaringMetadata = declaringMetadata;
    }
}

public class PropertyMetadata<TValue> : PropertyMetadata
{
    internal PropertyMetadata(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
    }

    internal PropertyMetadata(string name, Type type)
        : base(name, type)
    {
    }

    public PropertyMetadataRef<TValue> Ref()
    {
        return null;
    }
}

public class PropertyMetadata<TValue, TEntity> : PropertyMetadata<TValue>
{

    internal PropertyMetadata(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
    }
}

public static class P<TEntity>
{
    /// 这里需要避免使用重载方法导致回返值不同，否则生成器无法识别

    public static PropertyMetadata<TValue, TEntity> Property<TValue>(
        Expression<Func<TEntity, TValue>> propertyExpr,
        Action<PropertyMetadata<TValue, TEntity>>? buildAction = null)
    {
        var propertyInfo = Reflect<TEntity>.GetPropertyInfo(propertyExpr);

        var p = new PropertyMetadata<TValue, TEntity>(propertyInfo);

        buildAction?.Invoke(p);

        return p;
    }

    public static PropertyMetadata<TValue, TEntity> RegisterComputed<TValue>(Expression<Func<TEntity, TValue>> generateExpression)
    {
        return null;
    }

    public static PropertyMetadata<TValue, TEntity> ArrayProperty<TValue>(
        Expression<Func<TEntity, IEnumerable<TValue>>> propertyExpr,
        Action<PropertyMetadata<TValue, TEntity>>? buildAction = null)
    {
        var propertyInfo = Reflect<TEntity>.GetPropertyInfo(propertyExpr);

        var p = new PropertyMetadata<TValue, TEntity>(propertyInfo);

        buildAction?.Invoke(p);

        return p;
    }
}

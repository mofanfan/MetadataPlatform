using MetaModels.Reflaction;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaModels.Entities;

public class PropertyMetadata
{
    public string Name { get; protected set; }
    public Type Type { get; protected set; }

    /// <summary>
    /// 必填
    /// </summary>
    public bool Required { get; protected set; }

    public IReadOnlyDictionary<string, object?> Annotations => _annotations;

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
    }

    internal PropertyMetadata(string name, Type type)
    {
        Name = name;
        Type = type;
    }

    public void SetAnnonation(string name, object? value)
    {
        _annotations[name] = value;
    }

    public void RemoveAnnonation(string name)
    {
        _annotations.Remove(name);
    }

    public void WithRequired(bool b)
    {
        Required = b;
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

    public PropertyMetadata<TValue> CodingJoinable()
    {
        return this;
    }
}

public class PropertyMetadata<TValue, TEntity> : PropertyMetadata<TValue>
{

    internal PropertyMetadata(PropertyInfo propertyInfo)
        : base(propertyInfo)
    {
    }

    internal PropertyMetadata(string name, Type type)
        : base(name, type)
    {
    }

    public PropertyMetadata<TResult, TEntity> JoinTable<TOuter, TKey, TResult>(
        Expression<Func<TOuter, TKey>> outerKeySelector,
        Expression<Func<TEntity, TKey>> innerKeySelector,
        Expression<Func<TOuter, TResult>> resultSelector)
    {
        // return new PropertyMetadata<TResult, TEntity>();
        return null;
    }
}

public class VariadicPropertyHolder
{
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

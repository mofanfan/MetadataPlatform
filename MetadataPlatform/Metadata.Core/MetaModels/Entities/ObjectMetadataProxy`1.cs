using System.Linq.Expressions;

namespace MetaModels.Entities;

internal class ObjectMetadataProxy<T>
    : IObjectMetadataProxy<T>
    where T : PropertyMetadata
{
    private readonly T _target;

    public ObjectMetadataProxy(T metadata)
    {
        _target = metadata;
    }

    public IObjectMetadataProxy<T> WithName(string name)
    {
        return this;
    }

    public void Configure<TProperty>(Expression<Func<T, TProperty>> propertyExpr, Action<IPropertyMetadataProxy<TProperty>> buildAction)
        where TProperty : PropertyMetadata
    {
        var property = propertyExpr.Compile().Invoke(_target);
        var proxy = new PropertyMetadataProxy<TProperty>(property);
        buildAction?.Invoke(proxy);
    }
}

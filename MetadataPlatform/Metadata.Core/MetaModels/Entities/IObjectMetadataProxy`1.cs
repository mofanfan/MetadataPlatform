using System.Linq.Expressions;

namespace MetaModels.Entities;

public interface IObjectMetadataProxy<T>
{
    IObjectMetadataProxy<T> WithName(string name);
    void Configure<TProperty>(Expression<Func<T, TProperty>> propertyExpr, Action<IPropertyMetadataProxy<TProperty>> buildAction)
        where TProperty : PropertyMetadata;
}

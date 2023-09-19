namespace MetaModels.Entities;

public abstract class ObjectConfigurer<T>
{
}

public interface IObjectMetadataConfigurer<T>
    where T : ObjectMetadata
{
    void Configure(IObjectMetadataProxy<T> metadata);
}

public interface IPropertyMetadataProxy<T>
{
    IPropertyMetadataProxy<T> WithLabel(string label);
    IPropertyMetadataProxy<T> WithRequired(bool required = true);
    IPropertyMetadataProxy<T> WithMaintenanceOwners(MaintenanceOwners owners);
    IPropertyMetadataProxy<T> WithMaintenanceKind(MaintenanceKind kind);
    IPropertyMetadataProxy<T> WithPartOfCoding(bool partOfCoding = true);
}

internal class PropertyMetadataProxy<T> : IPropertyMetadataProxy<T>
    where T: PropertyMetadata
{
    private readonly T _property;

    public PropertyMetadataProxy(T property)
    {
        _property = property;
    }

    public IPropertyMetadataProxy<T> WithLabel(string label)
    {
        _property.Label = label;
        return this;
    }

    public IPropertyMetadataProxy<T> WithRequired(bool required)
    {
        _property.Required = required;
        return this;
    }

    public IPropertyMetadataProxy<T> WithMaintenanceOwners(MaintenanceOwners owners)
    {
        _property.MaintenanceOwners = owners;
        return this;
    }

    public IPropertyMetadataProxy<T> WithMaintenanceKind(MaintenanceKind kind)
    {
        _property.MaintenanceKind = kind;
        return this;
    }

    public IPropertyMetadataProxy<T> WithPartOfCoding(bool partOfCoding)
    {
        _property.PartOfCoding = partOfCoding;
        return this;
    }
}

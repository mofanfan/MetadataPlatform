namespace MetaModels.Entities;

public abstract class ObjectConfigurer<T>
{
}

public abstract class ObjectMetadataConfigurer
{
    protected void Configure<T>(T property, Action<IPropertyMetadataBuilder<T>> buildAction)
    {
        var builder = new PropertyMetadataBuilder<T>();
        buildAction.Invoke(builder);
    }
}

public interface IPropertyMetadataBuilder<T>
{

}

internal class PropertyMetadataBuilder<T> : IPropertyMetadataBuilder<T>
{
}
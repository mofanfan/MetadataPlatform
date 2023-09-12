namespace MetaModels.Entities;

public static class PropertyMetadataExtensions
{
    public static T WithGroup<T>(this T propertyMetadata, string groupName)
        where T : PropertyMetadata
    {
        if (string.IsNullOrEmpty(groupName)) {
            throw new ArgumentNullException(nameof(groupName));
        }

        propertyMetadata.SetAnnonation("#GroupName", groupName);

        return propertyMetadata;
    }
}

using MetaModels.Entities;

namespace Metadata.Demo.Entities;

public class SaleMaterial
{
}

public partial class SaleMaterialMetadataRegister
    : ObjectMetadataRegister<SaleMaterial>
{
    public static readonly ObjectMetadataRef<MaterialMetadata> Material = new();

    public void Configure(IObjectMetadataProxy<SaleMaterialMetadata> metadata)
    {
         
    }
}

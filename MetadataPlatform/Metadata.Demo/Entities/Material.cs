using MetaModels.Entities;
using System;
using System.Collections.Generic;

namespace Metadata.Demo.Entities;

public class UnitDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public enum G01Enum
{
    Value1,
    Value2,
    Value3,
}

public class Material
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public G01Enum G01 { get; set; }
    public string Name { get; set; }
    public Guid UnitId { get; set; }
    public IReadOnlyCollection<MaterialAssistMeasureUnit> MaterialAssistMeasureUnitItem {get;set;}
}

public enum UnitType
{
    Single,
    Multiple,
}

public class MaterialAssistMeasureUnit
{
    public Guid UnitId { get; set; }
    public decimal ConversionRate { get; set; }
}

public partial class MaterialMetadataRegister
    : ObjectMetadataRegister<Material>
{
    public static readonly PropertyMetadata<string> Code =
        P<Material>.Property(x => x.Code);

    public static readonly PropertyMetadata<string> Name =
        P<Material>.Property(x => x.Name);

    public static readonly PropertyMetadata<G01Enum> G01 =
        P<Material>.Property(x => x.G01);

    public static readonly PropertyMetadata<Guid> MasterUnit =
        P<Material>.Property(x => x.UnitId);

    public static readonly PropertyMetadata SlaveUnits =
        P<Material>.Property(x => x.MaterialAssistMeasureUnitItem);

    // public static readonly PropertyMetadata<UnitType> UnitType =
    //     P<Material>.RegisterComputed(
    //         x => x.MaterialAssistMeasureUnitItem.Count > 1 ? Entities.UnitType.Multiple : Entities.UnitType.Single);

    public void Configure(IObjectMetadataProxy<MaterialMetadata> metadata)
    {
        metadata.Configure(
            x => x.Code,
            p => p
                .WithRequired()
                .WithMaintenanceOwners(MaintenanceOwners.Manager));
    
        metadata.Configure(
            x => x.Name,
            p => p
                .WithRequired()
                .WithMaintenanceOwners(MaintenanceOwners.Manager)
                .WithMaintenanceKind(MaintenanceKind.General)
                .WithPartOfCoding());
    }
}

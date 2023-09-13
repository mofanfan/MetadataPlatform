using Metadata.Core.MetaModels.Attributes;
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

public class MaterialAssistMeasureUnit
{
    public Guid UnitId { get; set; }
    public decimal ConversionRate { get; set; }
}

[ObjectConfigurer(typeof(Material))]
public class MaterialMetadataDeclaration
{
    public static readonly PropertyMetadata Code =
        P<Material>.Property(x => x.Code, x => {
            x.WithRequired(true);
        });

    public static readonly PropertyMetadata Name =
        P<Material>.Property(x => x.Name);

    public static readonly PropertyMetadata G01 =
        P<Material>.Property(x => x.G01, x => {
            // 前台编辑
        });

    public static readonly PropertyMetadata MasterUnit =
        P<Material>.Property(x => x.UnitId);

    public static readonly PropertyMetadata SlaveUnits =
        P<Material>.Property(x => x.MaterialAssistMeasureUnitItem);
}


public partial class MaterialMetadataConfigurer
    : ObjectMetadataConfigurer
{
    public PropertyMetadata Code => MaterialMetadataDeclaration.Code;
    public PropertyMetadata Name => MaterialMetadataDeclaration.Name;
    public PropertyMetadata G01 => MaterialMetadataDeclaration.G01;
    public PropertyMetadata MasterUnit => MaterialMetadataDeclaration.MasterUnit;
    public PropertyMetadata SlaveUnits => MaterialMetadataDeclaration.SlaveUnits;

    public void Configure()
    {
        Configure(Code, builder => {
        });
    }
}

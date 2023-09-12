using Metadata.Core.MetaModels.Attributes;
using MetaModels.Entities;

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
public partial class MaterialConfigurer
{
    public static readonly PropertyMetadata Code =
        P<Material>.Property(x => x.Code, x => {
            x.WithRequired(true);
        });

    public readonly PropertyMetadata Name =
        P<Material>.Property(x => x.Name);

    public readonly PropertyMetadata G01 =
        P<Material>.Property(x => x.G01, x => {
            // 前台编辑
        });

    public readonly PropertyMetadata MasterUnit =
        P<Material>.Property(x => x.UnitId);

    public static readonly PropertyMetadata SlaveUnits =
        P<Material>.Property(x => x.MaterialAssistMeasureUnitItem);

    /// <summary>
    /// 自定义项
    /// </summary>
    public static readonly VariadicPropertyHolder CustomProperties;

    // public static readonly Test<MaterialAssistMeasureUnitConfigurer> A;
}

using MetaModels.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata.Demo.Entities;

#if false
public class Material
{
    public Type Type { get; } = typeof(Material);
    public Material__Properties Properties { get; } = new();
}

public class Material__Properties
{
    public PropertyMetadata Code = MaterialConfigurer.Code;
    public PropertyMetadata Unit => MaterialConfigurer.UnitName;
}

public class MaterialBo
{
    public List<object> Units { get; set; }

    public void Test()
    {
    }
}


public class ProductionTask
{
    public MaterialBo Material { get; set; }
}

public class NoneReasonProcutionTask
{
    public MaterialBo Material { get; set; }
}

#endif

namespace MetaModels.Templates;

public interface IPropertyGroupElement
{
}

public abstract class PropertyGroupElementContainer
{
    public IReadOnlyCollection<IPropertyGroupElement> Elements => _elements;
    private List<IPropertyGroupElement> _elements = new();
}

public class RootPropertyGroup : PropertyGroupElementContainer
{
    public BasicPropertyGroup Basic { get; } = new();
}

public class BasicPropertyGroup : PropertyGroupElementContainer
{
    public MeasurePropertyGroup Measure { get; } = new();
    public PropertyPropertyGroup Property { get; } = new();
}

public class MeasurePropertyGroup : PropertyGroupElementContainer
{
    public MeasureUnitPropertyGroup MeasureUnit { get; } = new();
    public SenceUnitPropertyGroup SenceUnit { get; } = new();
    public PackagingUnitPropertyGroup PackagingUnit { get; } = new();
}

public class MeasureUnitPropertyGroup : PropertyGroupElementContainer
{

}

public class SenceUnitPropertyGroup : PropertyGroupElementContainer
{

}

public class PackagingUnitPropertyGroup : PropertyGroupElementContainer
{

}

public class PropertyPropertyGroup : PropertyGroupElementContainer
{

}
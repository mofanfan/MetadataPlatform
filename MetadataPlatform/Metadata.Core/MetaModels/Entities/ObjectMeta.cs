namespace MetaModels.Entities;

public abstract class ObjectMeta
{
    /// <summary>
    /// 对象主实体类型
    /// </summary>
    public abstract Type EntityType { get; }

    /// <summary>
    /// 对象所有相关实体（不含引用项）
    /// </summary>
    public abstract IEnumerable<Type> EntityTypes { get; }
}

public abstract class PropertyMeta
{
    /// <summary>
    /// 项值运行时类型
    /// </summary>
    public abstract Type Type { get; }
    public abstract string Name { get; }
    public abstract string Label { get; protected set; }
}

public class PropertyMeta<T> : PropertyMeta
{
    public override Type Type => typeof(T);
    public override string Name { get; }
    public override string Label { get; protected set; }

    public PropertyMeta(string name)
    {
        Name = name;
        Label = name;
    }

    public PropertyMeta<T> HasLabel(string i18nRes)
    {
        // TODO:
        Label = i18nRes;

        return this;
    }
}
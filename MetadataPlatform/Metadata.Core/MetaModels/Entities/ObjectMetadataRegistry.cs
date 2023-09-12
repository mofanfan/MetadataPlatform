namespace MetaModels.Entities;

/// <summary>
/// 可注入服务
/// 对象注册表
/// </summary>
internal class ObjectMetadataRegistry
{
    public TConfigurer? FindMetadata<TConfigurer>()
        where TConfigurer : class
    {
        return null;
    }

    public TConfigurer GetMetadata<TConfigurer>()
        where TConfigurer : class
    {
        throw new NotImplementedException();
    }
}

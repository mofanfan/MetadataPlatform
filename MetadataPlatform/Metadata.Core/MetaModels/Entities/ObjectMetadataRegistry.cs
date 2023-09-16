using MetaModels.Options;
using Microsoft.Extensions.Options;
using System.Linq;

namespace MetaModels.Entities;

/// <summary>
/// 可注入服务
/// 对象注册表
/// </summary>
internal class ObjectMetadataRegistry
{
    private readonly ObjectMetadataOptions _options;
    private readonly Dictionary<string, ObjectMetadata> _objectMetadataById = new();
    private readonly Dictionary<Type, ObjectMetadata> _objectMetadataByEntityType = new();

    public ObjectMetadataRegistry(IOptions<ObjectMetadataOptions> options)
    {
        _options = options.Value;
    }

    public ObjectMetadataRegistry(ObjectMetadataOptions options)
    {
        _options = options;
    }

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

    public void Build()
    {
        var buildGraph = new Dictionary<Type, BuildGraphNode>();

        foreach (var item in _options.ObjectMetadataTypeMap) {
            var registerType = item.Key;
            var metadataType = item.Value;
            if (!buildGraph.TryGetValue(metadataType, out BuildGraphNode? graphNode)) {
                graphNode = new BuildGraphNode(registerType, metadataType);
                buildGraph.Add(metadataType, graphNode);
            }

            if (graphNode.Completed) {
                continue;
            }

            Build(graphNode);
        }
    }

    private void Build(BuildGraphNode graphNode)
    {
        var metadata = Activator.CreateInstance(graphNode.MetadataType);
        var register = Activator.CreateInstance(graphNode.RegisterType);
        var metadataProxyType = typeof(ObjectMetadataProxy<>).MakeGenericType(graphNode.MetadataType);
        var metadataProxy = Activator.CreateInstance(metadataProxyType, metadata);
        var configureMethod = graphNode.RegisterType.GetMethod("Configure", new Type[] { metadataProxyType }) ?? throw new NotImplementedException();
        configureMethod.Invoke(register, new object?[] { metadataProxy });
    }

    private class BuildGraphNode
    {
        public bool Completed { get; set; }

        public Type RegisterType { get; }
        public Type MetadataType { get; }

        public BuildGraphNode(Type registerType, Type metadataType)
        {
            RegisterType = registerType;
            MetadataType = metadataType;
        }
    }

    private Type? FindGenericInterface(Type type, Type genericInterfaceType)
    {
        var interfaces = type.GetInterfaces();
        return type.GetInterfaces()
            .Where(x => x.IsGenericType && x == genericInterfaceType)
            .SingleOrDefault();
    }

    private class Resolver
    {
        private readonly ObjectMetadataOptions _options;
        private readonly Stack<Type> _registerStack = new();

        public Resolver(ObjectMetadataOptions options)
        {
            _options = options;
        }

        public void Resolve()
        {
            foreach (var item in _options.ObjectMetadataTypeMap) {

            }
        }

        private void Resolve(Type registerType, Type metadataType)
        {
            if (_registerStack.Contains(registerType)) {
                throw new Exception();
            }
        }
    }
}

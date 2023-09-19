using MetaModels.Options;
using Microsoft.Extensions.Options;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

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

    public void Build()
    {
        Resolver resolver = new(_options);
        resolver.Resolve();
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

    private class Resolver
    {
        private readonly ObjectMetadataOptions _options;
        private readonly Stack<Type> _metadataStack = new();
        private readonly List<ObjectMetadata> _objectMetadatas = new();
        private readonly Dictionary<Type, ObjectMetadata> _metadataMap = new();
        private readonly Dictionary<Type, MethodInfo> _objectMetadataByEntityType = new();

        public Resolver(ObjectMetadataOptions options)
        {
            _options = options;

            var methods = typeof(Resolver).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var method in methods) {
                if (method.Name != nameof(SetupObjectMetadataProperty)) {
                    continue;
                }
                var parameters = method.GetParameters();
                _objectMetadataByEntityType.Add(parameters[1].ParameterType, method);
            }
        }

        public void Resolve()
        {
            foreach (var item in _options.ObjectMetadataTypeMap) {
                Resolve(item.Key, item.Value);
            }
        }

        private ObjectMetadata Resolve(Type metadataType)
        {
            if (_metadataMap.TryGetValue(metadataType, out var metadata)) {
                return metadata;
            }
            if (_options.ObjectMetadataTypeMap2.TryGetValue(metadataType, out var registerType)) {
                return Resolve(registerType, metadataType);
            }

            throw new NotImplementedException();
        }

        private ObjectMetadata Resolve(Type registerType, Type metadataType)
        {
            if (_metadataStack.Contains(metadataType)) {
                var messageBuilder = new StringBuilder();

                messageBuilder.AppendLine("元数据循环依赖：");

                foreach (var name in _metadataStack.Select(x => x.FullName)) {
                    messageBuilder.AppendLine($"    {name}");
                }

                messageBuilder.AppendLine($">>> {metadataType.FullName}");

                throw new Exception(messageBuilder.ToString());
            }

            _metadataStack.Push(metadataType);

            var metadata = Activator.CreateInstance(metadataType) as ObjectMetadata;
            if (metadata == null) {
                throw new ArgumentNullException(nameof(metadata));
            }

            var configurer = Activator.CreateInstance(registerType);
            var configurerType = typeof(IObjectMetadataConfigurer<>).MakeGenericType(metadataType);

            ConfigureObjectMetadataProperties(metadataType, metadata);
            InvokeConfigurer(configurer, metadata);

            if (!_metadataMap.TryAdd(metadataType, metadata)) {
                // TODO:
            }

            return metadata;
        }

        private void ConfigureObjectMetadataProperties(
            Type objectMetadataType,
            ObjectMetadata objectMetadata)
        {
            var properties = objectMetadataType.GetProperties();

            foreach (var property in properties) {
                SetupObjectMetadataProperty(objectMetadata, property);
            }
        }

        private void SetupObjectMetadataProperty(
            ObjectMetadata objectMetadata,
            PropertyInfo property)
        {
            var propertyValue = property.GetValue(objectMetadata);
            if (propertyValue == null) {
                throw new ArgumentNullException(nameof(propertyValue));
            }

            var setupAction = typeof(Resolver).GetMethod(
                nameof(SetupObjectMetadataProperty),
                BindingFlags.Instance | BindingFlags.NonPublic,
                new Type[] {
                    typeof(ObjectMetadata),
                    property.PropertyType,
                });

            if (setupAction == null) {
                var builder = new StringBuilder();
                builder.AppendLine("没有合适的方法安装对象属性元数据。");
                builder.AppendLine($"- ObjectMetadataType: {objectMetadata.GetType().FullName}");
                builder.AppendLine($"- EntityType: {objectMetadata.GetEntityType().FullName}");
                builder.AppendLine($"- Property: {property.Name} ({property.PropertyType})");

                throw new ArgumentNullException(
                    nameof(setupAction),
                    builder.ToString());
            }

            setupAction.Invoke(this, new object[] { objectMetadata, propertyValue });
        }

        private void SetupObjectMetadataProperty(
            ObjectMetadata objectMetadata,
            PropertyMetadata propertyMetadata)
        {
            propertyMetadata.ConfigureInternal(objectMetadata);
        }

        private void SetupObjectMetadataProperty(
            ObjectMetadata objectMetadata,
            ObjectMetadataRef propertyMetadata)
        {
            var targetObjectMetadata = Resolve(propertyMetadata.TargetType);
            var methodInfo = propertyMetadata.GetType().GetMethod(
                nameof(ObjectMetadataRef<ObjectMetadata>.ConfigureInternal),
                BindingFlags.Instance | BindingFlags.NonPublic,
                new Type[] { propertyMetadata.TargetType, propertyMetadata.TargetType });
            methodInfo.Invoke(propertyMetadata, new object[] { objectMetadata, targetObjectMetadata });
        }

        private void InvokeConfigurer(
            object configurer,
            ObjectMetadata objectMetadata)
        {
            var configurerType = typeof(IObjectMetadataConfigurer<>).MakeGenericType(objectMetadata.GetType());
            if (!configurerType.IsInstanceOfType(configurer)) {
                throw new ArgumentException("Configurer not implemented IObjectMetadataConfigurer<>");
            }

            var methodInfo = configurerType.GetMethod("Configure");
            if (methodInfo == null) {
                throw new ArgumentNullException();
            }

            var proxyType = typeof(ObjectMetadataProxy<>).MakeGenericType(objectMetadata.GetType());

            var proxy = Activator.CreateInstance(proxyType, objectMetadata);

            methodInfo.Invoke(configurer, new object[] { proxy });
        }
    }
}

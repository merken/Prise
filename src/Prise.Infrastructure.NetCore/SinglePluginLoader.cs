using System;
using System.Linq;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class SinglePluginLoader<T> : IPluginLoader<T>
    {
        private readonly IPluginLoadOptions<T> pluginLoadOptions;

        public SinglePluginLoader(IPluginLoadOptions<T> pluginLoadOptions)
        {
            this.pluginLoadOptions = pluginLoadOptions;
        }

        public async Task<T> Load()
        {
            // TODO
            var pluginAssembly = await pluginLoadOptions.AssemblyLoader.Load(pluginLoadOptions.PluginAssemblyName);
            var pluginType = pluginAssembly
                            .GetTypes()
                            .Where(t => t.CustomAttributes
                                .Any(c => c.AttributeType.Name == typeof(Prise.Infrastructure.PluginAttribute).Name
                                && (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == typeof(T).Name)).FirstOrDefault();

            var bootstrapperType = pluginAssembly
                                        .GetTypes()
                                        .Where(t => t.CustomAttributes
                                            .Any(c => c.AttributeType.Name == typeof(Prise.Infrastructure.PluginBootstrapperAttribute).Name &&
                                            (c.NamedArguments.First(a => a.MemberName == "PluginType").TypedValue.Value as Type).Name == pluginType.Name)).FirstOrDefault();

            var pluginFactoryMethod = pluginType.GetMethods()
                                        .Where(m => m.CustomAttributes
                                            .Any(c => c.AttributeType.Name == typeof(Prise.Infrastructure.PluginFactoryAttribute).Name)).FirstOrDefault();

            IPluginBootstrapper bootstrapper = null;
            if (bootstrapperType != null)
            {
                var remoteBootstrapperInstance = pluginLoadOptions.Activator.CreateRemoteBootstrapper(bootstrapperType);
                if (remoteBootstrapperInstance as IPluginBootstrapper == null)
                    throw new NotSupportedException("Version type mismatch");
                bootstrapper = remoteBootstrapperInstance as IPluginBootstrapper;
            }

            var remoteObject = pluginLoadOptions.Activator.CreateRemoteInstance(pluginType, bootstrapper, pluginFactoryMethod);
            return CreateProxy(remoteObject);
        }

        public Task<T[]> LoadAll()
        {
            throw new System.NotImplementedException();
        }

        private T CreateProxy(object remoteObject)
        {
            var proxy = PluginProxy<T>.Create();

            ((PluginProxy<T>)proxy)
               .SetRemoteObject(remoteObject)
               .SetParameterConverter(pluginLoadOptions.ParameterConverter)
               .SetResultConverter(pluginLoadOptions.ResultConverter);

            return (T)proxy;
        }
    }
}
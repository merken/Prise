// namespace Prise.Infrastructure.NetCore
// {
//     public class PluginProxyBuilder<T>
//     {
//         private IRemotePluginActivator activator;
//         private IParameterConverter parameterConverter;
//         private IResultConverter resultConverter;

//         public static PluginProxyBuilder<T> Create() => new PluginProxyBuilder<T>();

//         public PluginProxyBuilder<T> WithDefaultSettings()
//         {
//             return this
//                 .WithParameterConverter(new NewtonsoftParameterConverter())
//                 .WithParameterConverter(new NewtonsoftParameterConverter())
//                 .WithResultConverter(new BinaryFormatterResultConverter());
//         }

//         public PluginProxyBuilder<T> WithActivator(IRemotePluginActivator activator)
//         {
//             this.activator = activator;
//             return this;
//         }

//         public PluginProxyBuilder<T> WithParameterConverter(IParameterConverter converter)
//         {
//             this.parameterConverter = converter;
//             return this;
//         }

//         public PluginProxyBuilder<T> WithResultConverter(IResultConverter converter)
//         {
//             this.resultConverter = converter;
//             return this;
//         }

//         public T Build()
//         {
//             var remoteObject = activator.CreateRemoteInstance();
//             var proxy = PluginProxy<T>.Create();

//             ((PluginProxy<T>)proxy)
//                 .SetRemoteObject(remoteObject)
//                 .SetParameterConverter(parameterConverter)
//                 .SetResultConverter(resultConverter);
                
//             return (T)proxy;
//         }
//     }
// }
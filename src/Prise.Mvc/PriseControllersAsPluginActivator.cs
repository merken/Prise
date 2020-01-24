using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Prise.Infrastructure;
using Prise.Plugin;

namespace Prise.Mvc
{
    public class PriseControllersAsPluginActivator<T> : IControllerActivator
    {
        public object Create(ControllerContext context)
        {
            var pluginLoadOptions = context.HttpContext.RequestServices.GetRequiredService<IPluginLoadOptions<T>>();
            var cache = context.HttpContext.RequestServices.GetRequiredService<IPluginCache<T>>();
            var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();

            foreach (var pluginAssembly in cache.GetAll())
            {
                if (pluginAssembly.GetTypes().Any(t => t.Name == controllerType.Name))
                {
                    var pluginActivationContext = pluginLoadOptions.PluginActivationContextProvider.ProvideActivationContext(controllerType, pluginAssembly);
                    IPluginBootstrapper bootstrapperProxy = null;
                    if (pluginActivationContext.PluginBootstrapperType != null)
                    {
                        var remoteBootstrapperInstance = pluginLoadOptions.Activator.CreateRemoteBootstrapper(pluginActivationContext.PluginBootstrapperType, pluginAssembly);
                        var remoteBootstrapperProxy = pluginLoadOptions.ProxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);
                        bootstrapperProxy = remoteBootstrapperProxy;
                    }

                    // This will use the parameterless ctor
                    // But it should use the IFeatureServiceProvider from the IFeatureServiceCollection
                    var remoteController = pluginLoadOptions.Activator.CreateRemoteInstance(
                        pluginActivationContext,
                        bootstrapperProxy);
                        //TODO CHECK
                        //controllerType,
                        //pluginAssembly,
                        //bootstrapperProxy,
                        //pluginActivationContext.PluginFactoryMethod);

                    // TODO Create own Controller Activator

                    var controllerContext = new ControllerContext();
                    controllerContext.HttpContext = context.HttpContext;
                    var controllerContextProperty = controllerType.GetProperty("ControllerContext");
                    controllerContextProperty.SetValue(remoteController, controllerContext);

                    return remoteController;
                }
            }

            // Use MSFT's own activator utilities to create a controller instance
            // This avoids us to require to register all controllers as services
            return ActivatorUtilities.CreateInstance(context.HttpContext.RequestServices, controllerType);
        }

        public void Release(ControllerContext context, object controller)
        {
            (controller as IDisposable)?.Dispose();
        }
    }
}

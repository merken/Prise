using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Prise.Activation;
using Prise.Infrastructure;
using Prise.Plugin;
using Prise.Proxy;
using Prise.Caching;

namespace Prise.Mvc
{
    public class DefaultPriseMvcControllerActivator : IControllerActivator
    {
        public object Create(ControllerContext context)
        {
            var cache = context.HttpContext.RequestServices.GetRequiredService<IPluginCache>();
            var controllerType = context.ActionDescriptor.ControllerTypeInfo.AsType();

            foreach (var cachedPluginAssembly in cache.GetAll())
            {
                var pluginAssembly = cachedPluginAssembly.AssemblyShim.Assembly;
                var pluginControllerType = pluginAssembly.GetTypes().FirstOrDefault(t => t.Name == controllerType.Name);
                if (pluginControllerType != null)
                {
                    var activatorContextProvider = context.HttpContext.RequestServices.GetRequiredService<IPluginActivationContextProvider>();
                    var remotePluginActivator = context.HttpContext.RequestServices.GetRequiredService<IRemotePluginActivator>();
                    var proxyCreator = context.HttpContext.RequestServices.GetRequiredService<IPluginProxyCreator>();
                    var resultConverter = context.HttpContext.RequestServices.GetRequiredService<IResultConverter>();
                    var parameterConverter = context.HttpContext.RequestServices.GetRequiredService<IParameterConverter>();

                    object controller = null;
                    IPluginBootstrapper bootstrapperProxy = null;
                    IServiceCollection hostServices = new ServiceCollection();
                    foreach (var hostServiceType in cachedPluginAssembly.HostTypes)
                        hostServices.Add(new ServiceDescriptor(hostServiceType, context.HttpContext.RequestServices.GetRequiredService(hostServiceType)));

                    var pluginActivationContext = activatorContextProvider.ProvideActivationContext(pluginControllerType, cachedPluginAssembly.AssemblyShim);

                    if (pluginActivationContext.PluginBootstrapperType != null)
                    {
                        var remoteBootstrapperInstance = remotePluginActivator.CreateRemoteBootstrapper(pluginActivationContext, hostServices);

                        var remoteBootstrapperProxy = proxyCreator.CreateBootstrapperProxy(remoteBootstrapperInstance);

                        bootstrapperProxy = remoteBootstrapperProxy;
                    }

                    controller = remotePluginActivator.CreateRemoteInstance(
                        pluginActivationContext,
                        bootstrapperProxy,
                        hostServices: hostServices
                    );

                    var controllerContext = new ControllerContext();
                    controllerContext.HttpContext = context.HttpContext;
                    var controllerContextProperty = controllerType.GetProperty("ControllerContext");
                    controllerContextProperty.SetValue(controller, controllerContext);

                    return controller;
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

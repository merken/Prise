using System;
using System.Threading.Tasks;

namespace Prise.Activation
{
    public interface IPluginActivator: IDisposable
    {
        Task<T> ActivatePlugin<T>(IPluginActivationOptions pluginActivationMetaData);
    }
}
using System.Threading.Tasks;

namespace Prise.Activation
{
    public interface IPluginActivator
    {
        Task<T> ActivatePlugin<T>(IPluginActivationOptions pluginActivationMetaData);
    }
}
using Contract;
using Prise.Infrastructure;

namespace Component
{
    [Plugin(PluginType=typeof(IAppComponent))]
    public class DialogComponent : IAppComponent
    {
        public void Activate(ComponentInput input)
        {
        }

        // TODO Shared services
    }
}
using System;
using System.Threading.Tasks;
using Prise.Console.Contract;

namespace Prise.Plugin.Multiple
{
    [Plugin(PluginType = typeof(IMultiplePlugin))]
    public class SayCPlugin : IMultiplePlugin
    {
        public async Task<PluginObject> GetData(PluginObject input)
        {
            await Task.Delay(1000);
            return new PluginObject
            {
                Number = 1000,
                Text = input.Text + "C"
            };
        }
    }
}

using System;
using System.Threading.Tasks;
using Prise.Console.Contract;

namespace Prise.Plugin.Multiple
{
    [Plugin(PluginType = typeof(IMultiplePlugin))]
    public class SayAPlugin : IMultiplePlugin
    {
        public async Task<PluginObject> GetData(PluginObject input)
        {
            await Task.Delay(2000);
            return new PluginObject
            {
                Number = 2000,
                Text = input.Text + "A"
            };
        }
    }
}

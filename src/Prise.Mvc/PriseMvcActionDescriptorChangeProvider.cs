using System.Threading;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Primitives;

namespace Prise.Mvc
{
    public class PriseMvcActionDescriptorChangeProvider : IActionDescriptorChangeProvider, IPriseMvcActionDescriptorChangeProvider
    {
        public CancellationTokenSource TokenSource { get; private set; }

        public bool HasChanged { get; set; }

        public IChangeToken GetChangeToken()
        {
            TokenSource = new CancellationTokenSource();
            return new CancellationChangeToken(TokenSource.Token);
        }

        public void TriggerPluginChanged()
        {
            HasChanged = true;
            TokenSource.Cancel();
        }
    }
}

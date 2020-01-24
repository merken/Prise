using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
#if NETCORE3_0
    public class BreakTheServerTests : PluginTestBase,
         IClassFixture<AppHostWebApplicationFactory>
    {
        public BreakTheServerTests(
                 AppHostWebApplicationFactory factory) : base(factory) { }

        [Fact]
        public async Task BreakWithLoop()
        {
            var tasks = new List<Task<string>>();
            for (var i = 0; i < 250; i++)
            {
                tasks.Add(GetRaw(_client, "PluginA", "/disco"));
            }

            var results = await Task.WhenAll(tasks.ToArray());

            Assert.All<string>(results, s => Assert.Equal("AdditionCalculationPlugin,ZAdditionPlusOneCalculationPlugin", s));

        }
    }
#endif
}

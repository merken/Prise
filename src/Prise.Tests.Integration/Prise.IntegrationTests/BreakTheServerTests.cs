using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Prise.IntegrationTests
{
#if NETCORE3_1
    public class BreakTheServerTests : CalculationPluginTestsBase
    {
        public BreakTheServerTests() : base(AppHostWebApplicationFactory.Default()) { }

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

using System.Collections.Generic;
using System.Threading.Tasks;
using Prise.Plugin;

namespace Prise.Tests.Plugins
{
    [Plugin(PluginType = typeof(ITestPlugin))]
    public class TestPluginActivation : ITestPlugin
    {
        private List<TestDto> data;

        [PluginActivated]
        public void Activated()
        {
            this.data = new List<TestDto>
            {
                new TestDto
                {
                    Name = "TestPluginB Activated!"
                }
            };
        }

        public Task<TestDto> Bar(TestDto dto)
        {
            return Task.FromResult(dto);
        }

        public Task<IEnumerable<TestDto>> Bars(IEnumerable<TestDto> dtos)
        {
            return Task.FromResult(dtos);
        }

        public TestDto Foo(TestDto dto)
        {
            return dto;
        }

        public IEnumerable<TestDto> Foos(IEnumerable<TestDto> dtos)
        {
            return this.data;
        }

        public async Task<IEnumerable<TestDto>> GetData(string filter)
        {
            return this.data;
        }

        public async Task<string> GetStringAsync()
        {
            return "String";
        }
    }
}
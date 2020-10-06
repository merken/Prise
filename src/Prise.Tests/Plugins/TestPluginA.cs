using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Prise.Plugin;

namespace Prise.Tests.Plugins
{
    [Plugin(PluginType = typeof(ITestPlugin))]
    public class TestPluginA : ITestPlugin
    {
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
            return dtos;
        }

        public async Task<IEnumerable<TestDto>> GetData(string filter)
        {
            return Enumerable.Empty<TestDto>();
        }

        public async Task<string> GetStringAsync()
        {
            return "String";
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise.Tests.Plugins
{
    public class TestDto
    {
        public IEnumerable<TestDto> Children { get; set; }
        public string Name { get; set; }
        public int Number { get; set; }
    }

    public interface ITestPlugin
    {
        Task<string> GetStringAsync();
        TestDto Foo(TestDto dto);
        IEnumerable<TestDto> Foos(IEnumerable<TestDto> dto);
        Task<TestDto> Bar(TestDto dto);
        Task<IEnumerable<TestDto>> Bars(IEnumerable<TestDto> dto);
        Task<IEnumerable<TestDto>> GetData(string filter);
    }
}
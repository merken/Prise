using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Prise.IntegrationTestsContract
{
    public class Data
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public IEnumerable<Data> Children { get; set; }
    }

    public interface IAuthenticatedDataService
    {
        Task<IEnumerable<Data>> GetData(string token);
    }
}

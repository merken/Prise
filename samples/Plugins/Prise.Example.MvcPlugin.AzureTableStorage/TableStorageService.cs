
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Prise.Example.Contract;
using Prise.Plugin;

namespace Prise.Example.MvcPlugin.AzureTableStorage
{
    public interface ITableStorageService
    {
        Task<IEnumerable<MyDto>> GetAll();
    }

    public class TableStorageService : TableStorageProviderBase<DataEntity>, ITableStorageService
    {
        public TableStorageService(TableConfig config) : base(config) { }
        
        public async Task<IEnumerable<MyDto>> GetAll()
        {
            var tableEntities = await this.GetAllItems();
            return tableEntities.Select(e => ToDto(e));
        }

        private MyDto ToDto(DataEntity e) => new MyDto
        {
            Number = e.Number,
            Text = e.Text
        };
    }
}
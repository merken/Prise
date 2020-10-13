using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Example.Contract;
using Prise.Plugin;
using System.Data;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Data.Common;

namespace MvcPlugin.DataStorage.Sql
{
    [ApiController]
    [Route("api/sql")]
    public class SqlDataController : ControllerBase
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(SqlDbContext))]
        private readonly SqlDbContext dbContext;

        [PluginActivated]
        public void OnActivated()
        {
            // TODO activation code... 
        }

        [HttpGet]
        public async Task<IEnumerable<MyDto>> Get()
        {
            return await dbContext.Data
                .AsNoTracking()
                .ToListAsync();
        }
    }
}
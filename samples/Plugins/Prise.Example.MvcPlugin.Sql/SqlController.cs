using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prise.Plugin;
using Prise.Example.Contract;
using System.Data.SqlClient;

namespace Prise.Example.MvcPlugin.Sql
{
    [ApiController]
    [Route("api/sql")]
    public class SqlController : ControllerBase
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(SqlDbContext))]
        private readonly SqlDbContext dbContext; 
       
        [HttpGet]
        public async Task<IEnumerable<MyDto>> Get()
        {
            return await dbContext.Data
                .AsNoTracking()
                .ToListAsync();
        }
    }
}

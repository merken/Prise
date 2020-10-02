using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Example.Contract;
using Prise.Plugin;

namespace MvcPlugin.DataStorage.Azure
{
    [ApiController]
    [Route("api/azure")]
    public class AzureDataController : ControllerBase
    {
        [PluginService(ProvidedBy = ProvidedBy.Plugin, ServiceType = typeof(ITableStorageService))]
        private readonly ITableStorageService service;

        [PluginActivated]
        public void OnActivated()
        {
            // TODO activation code... 
        }

        [HttpGet]
        public async Task<IEnumerable<MyDto>> Get()
        {
            return await this.service.GetAll();
        }
    }
}
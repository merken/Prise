using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Example.Contract;
using Example.Mvc.Razor.Models;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Prise.Mvc;
using Prise.Core;

namespace Example.Mvc.Razor.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ApplicationPartManager applicationPartManager;
        private readonly IMvcPluginLoader mvcPluginLoader;
        private readonly IPriseMvcActionDescriptorChangeProvider pluginChangeProvider;
        public HomeController(
            ILogger<HomeController> logger,
            ApplicationPartManager applicationPartManager,
            IMvcPluginLoader mvcPluginLoader,
            IPriseMvcActionDescriptorChangeProvider pluginChangeProvider)
        {
            this.logger = logger;
            this.applicationPartManager = applicationPartManager;
            this.mvcPluginLoader = mvcPluginLoader;
            this.pluginChangeProvider = pluginChangeProvider;
        }

        public async Task<IActionResult> Index()
        {
            return View(await GetHomeViewModel());
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [Route("home/enable/{pluginName}")]
        public async Task<IActionResult> Enable(string pluginName)
        {
            if (String.IsNullOrEmpty(pluginName))
                return NotFound();

            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(GetPathToDist());
            var pluginToEnable = pluginAssemblies.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p.AssemblyName) == pluginName);
            if (pluginToEnable == null)
                return NotFound();

            var pluginAssembly = await this.mvcPluginLoader.LoadPluginAssembly<IMvcPlugin>(pluginToEnable);

            this.applicationPartManager.ApplicationParts.Add(new PluginAssemblyPart(pluginAssembly.Assembly));
            
            this.pluginChangeProvider.TriggerPluginChanged();

            return Redirect("/");
        }

        [Route("home/disable/{pluginName}")]
        public async Task<IActionResult> Disable(string pluginName)
        {
            if (String.IsNullOrEmpty(pluginName))
                return NotFound();

            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(GetPathToDist());
            var pluginToDisable = pluginAssemblies.FirstOrDefault(p => Path.GetFileNameWithoutExtension(p.AssemblyName) == pluginName);
            if (pluginToDisable == null)
                return NotFound();

            var pluginAssemblyToDisable = Path.GetFileNameWithoutExtension(pluginToDisable.AssemblyName);
            var partToRemove = this.applicationPartManager.ApplicationParts.FirstOrDefault(a => a.Name == pluginAssemblyToDisable);

            this.applicationPartManager.ApplicationParts.Remove(partToRemove);
            await this.mvcPluginLoader.UnloadPluginAssembly<IMvcPlugin>(pluginToDisable);
            this.pluginChangeProvider.TriggerPluginChanged();

            return Redirect("/");
        }

        private async Task<HomeViewModel> GetHomeViewModel()
        {
            var applicationParts = this.applicationPartManager.ApplicationParts;
            var pluginAssemblies = await this.mvcPluginLoader.FindPlugins<IMvcPlugin>(GetPathToDist());

            var loadedPlugins = from plugin in pluginAssemblies
                                let pluginName = Path.GetFileNameWithoutExtension(plugin.AssemblyName)
                                let pluginType = plugin.PluginType
                                let pluginDescriptionAttribute = CustomAttributeData.GetCustomAttributes(pluginType).FirstOrDefault(c => c.AttributeType.Name == typeof(MvcPluginDescriptionAttribute).Name)
                                let pluginDescription = pluginDescriptionAttribute.NamedArguments.FirstOrDefault(a => a.MemberName == "Description").TypedValue.Value as string
                                join part in applicationParts
                                    on pluginName equals part.Name
                                    into pluginParts
                                from pluginPart in pluginParts.DefaultIfEmpty()
                                select new Example.Mvc.Razor.Models.Plugin
                                {
                                    Name = pluginName,
                                    Description = pluginDescription,
                                    IsEnabled = pluginPart != null
                                };

            return new HomeViewModel() { Plugins = loadedPlugins };
        }

        private static string GetPathToDist()
        {
            var pathToThisProgram = Assembly.GetExecutingAssembly() // this assembly location (/bin/Debug/netcoreapp3.1)
                                        .Location;
            var pathToExecutingDir = Path.GetDirectoryName(pathToThisProgram);
            return Path.GetFullPath(Path.Combine(pathToExecutingDir, "../../../../Plugins/dist"));
        }
    }
}

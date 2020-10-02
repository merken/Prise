using System.Collections.Generic;

namespace Example.Mvc.Controllers.Models
{
    public class Plugin
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class HomeViewModel
    {
        public IEnumerable<Plugin> Plugins { get; set; }
    }
}
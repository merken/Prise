using Microsoft.AspNetCore.Http;
using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyHost.Infrastructure
{
    public class HttpClientPluginSelector<T> : IPluginSelector<T>
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public HttpClientPluginSelector(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<Type> SelectPlugins(IEnumerable<Type> pluginTypes)
        {
            if (this.httpContextAccessor.HttpContext.Request.Headers.ContainsKey("starwars"))
                return pluginTypes.Where(t => t.Name.Contains("SWAPIRepository"));

            return pluginTypes.Where(t => !t.Name.Contains("SWAPIRepository"));
        }
    }
}

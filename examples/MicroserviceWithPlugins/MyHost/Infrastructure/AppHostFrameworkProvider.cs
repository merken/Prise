using Prise.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyHost.Infrastructure
{
    /// <summary>
    /// This is required for testing
    /// </summary>
    public class AppHostFrameworkProvider : IHostFrameworkProvider
    {
        public string ProvideHostFramwork() => typeof(AppHostFrameworkProvider).Assembly
            .GetCustomAttribute<TargetFrameworkAttribute>()?
            .FrameworkName;
    }
}

using System;
using Prise.Example.Contract;

namespace Prise.Example.AzureFunction
{
    public class EnvironmentConfigurationService : IConfigurationService
    {
        public string GetConfigurationValueForKey(string key)
        {
            return System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
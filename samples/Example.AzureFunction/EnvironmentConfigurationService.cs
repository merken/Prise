using System;
using Example.Contract;

namespace Example.AzureFunction
{
    public class EnvironmentConfigurationService : IConfigurationService
    {
        public string GetConfigurationValueForKey(string key)
        {
            return System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
using System;
using Prise.Console.Contract;

namespace Prise.Functions.Example
{
    public class EnvironmentConfigurationService : IConfigurationService
    {
        public string GetConfigurationValueForKey(string key)
        {
            return System.Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process);
        }
    }
}
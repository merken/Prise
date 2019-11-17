using System;
using System.Reflection;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblyLocation = ".";
            var assemblyName = "SQLPlugin";
            DependencyFile file;
            using (var stream = new StreamReader(Path.Join(assemblyLocation, $"{assemblyName}.deps.json")))
            {
                var json = stream.ReadToEnd();
                file = JsonSerializer.Deserialize<DependencyFile>(json);
            }

            Console.WriteLine(file.Libraries);
        }

    }

    public class DependencyFile
    {
        [JsonPropertyName("libraries")]
        public Dictionary<String, Library> Libraries { get; set; }
    }

    public class Library
    {
        [JsonPropertyName("type")]
        public String Type { get; set; }

        [JsonPropertyName("serviceable")]
        public bool Serviceable { get; set; }
    }

}

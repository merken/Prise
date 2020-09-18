using System;
using System.Linq;
using System.Reflection;
using Prise.Plugin.Sql.Legacy;
using Prise.Web;

namespace Prise.Console2
{
    class Program
    {
        static void Main(string[] args)
        {
            var sqlPlugin = new SqlStoragePlugin();
            typeof(SqlStoragePlugin).GetTypeInfo()
                            .DeclaredFields
                                .First(f => f.Name == "configurationService")
                                .SetValue(sqlPlugin, new AppSettingsConfigurationService());

            sqlPlugin.OnActivated();

            // TODO THIS WORKS BUT PLATFORM UNSUPPORTED ON NET CORE 2????
            System.Console.WriteLine(sqlPlugin.GetData(new Console.Contract.PluginObject
            {
                Text = "999"
            }).Result.Text);

        }
    }
}

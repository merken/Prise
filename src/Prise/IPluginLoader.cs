using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Prise
{
    public interface IPluginLoader
    {
        /// <summary>
        /// Looks for the first plugin of contract type <see cref="{T}"/> inside of the pathToPlugins directory recursively.
        /// The comparison is done on the last part of the path of the plugins.
        /// eg: "plugins/mypluginA" => mypluginA
        /// </summary>
        /// <param name="pathToPlugins">Directory to start looking for plugins</param>
        /// <param name="plugin">The name of the plugin to find. eg: mypluginA</param>
        /// <typeparam name="T">The plugin contract</typeparam>
        /// <returns>A <see cref="AssemblyScanResult"/> that contains all the required information in order to load the plugin.</returns>
        Task<AssemblyScanResult> FindPlugin<T>(string pathToPlugins, string plugin);

        /// <summary>
        /// Looks for all the plugins from a certain directory recursively
        /// </summary>
        /// <param name="pathToPlugins">Starting path to start searching plugins of contract type <see cref="{T}"/> </param>
        /// <typeparam name="T">The plugin contract</typeparam>
        /// <returns>A List of <see cref="AssemblyScanResult"/> that contains all the required information in order to load the plugins</returns>
        Task<IEnumerable<AssemblyScanResult>> FindPlugins<T>(string pathToPlugins);

        /// <summary>
        /// Loads the first implementation of the plugin contract from the <see cref="AssemblyScanResult"/>.
        /// Use this if you're certain that your plugin assemblies contain only 1 plugin.
        /// </summary>
        /// <param name="scanResult">The <see cref="AssemblyScanResult"/> from the FindPlugin, FindPlugins or FindPluginsAsAsyncEnumerable method.</param>
        /// <param name="hostFramework">The framework from the host application, optional.--></param>
        /// <param name="configure">A builder function that allows you to modify the PluginLoadContext before loading the plugin<, optional./param>
        /// <typeparam name="T">The plugin contract</typeparam>
        /// <returns>A fully loaded and usable plugin of type <see cref="{T}"/></returns>
        Task<T> LoadPlugin<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configure = null);

        /// <summary>
        /// Loads all the plugins from a specific <see cref="AssemblyScanResult"/>.
        /// </summary>
        /// <param name="scanResult">The <see cref="AssemblyScanResult"/> from the FindPlugin, FindPlugins or FindPluginsAsAsyncEnumerable method.</param>
        /// <param name="hostFramework">The framework from the host application, optional.--></param>
        /// <param name="configure">A builder function that allows you to modify the PluginLoadContext before loading the plugin<, optional./param>
        /// <typeparam name="T">The plugin contract</typeparam>
        /// <returns>A list of fully loaded and usable plugins of type <see cref="{T}"/></returns>
        Task<IEnumerable<T>> LoadPlugins<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configure = null);

        /// <summary>
        /// See <see cref="{FindPlugins}"/>
        /// This method returns an IAsyncEnumerable for you to use inside of an async foreach
        /// </summary>
        /// <param name="scanResult">The <see cref="AssemblyScanResult"/> from the FindPlugin, FindPlugins or FindPluginsAsAsyncEnumerable method.</param>
        /// <param name="hostFramework">The framework from the host application, optional.--></param>
        /// <param name="configure">A builder function that allows you to modify the PluginLoadContext before loading the plugin<, optional./param>
        /// <typeparam name="T">The plugin contract</typeparam>
        /// <returns>An IAsyncEnumerable of fully loaded and usable plugins of type <see cref="{T}"/></returns>
        IAsyncEnumerable<T> LoadPluginsAsAsyncEnumerable<T>(AssemblyScanResult scanResult, string hostFramework = null, Action<PluginLoadContext> configure = null);
    }
}

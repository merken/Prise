using System;
using System.Threading.Tasks;

namespace Prise.Infrastructure
{
    public interface IResultConverter : IDisposable
    {
        /// <summary>
        /// This method should convert a remote value into a local one.
        /// This method will be called in case the remote type is not a Task.
        /// </summary>
        /// <param name="localType">The return type of the host</param>
        /// <param name="remoteType">The return type of the plugin</param>
        /// <param name="value">The value from the plugin, this can be different from the remoteType if the plugin uses an old contract, in this case, convert to correct type.</param>
        /// <returns>A converted local instance</returns>
        object ConvertToLocalType(Type localType, Type remoteType, object value);

        /// <summary>
        /// This method should convert a remote value into a local one.
        /// This method will be called in case the remote type is a Task.
        /// </summary>
        /// <param name="localType">The return type of the host, Task<localType></param>
        /// <param name="remoteType">The return type of the plugin, Task<remoteType></param>
        /// <param name="task">The Task that holds the value from the plugin, this can be different from the remoteType if the plugin uses an old contract, in this case, convert to correct type.</param>
        /// <returns>A Task containing the local type with the remote value</returns>
        object ConvertToLocalTypeAsync(Type localType, Type remoteType, Task task);
    }
}
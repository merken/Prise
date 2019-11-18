using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise
{
    /// <summary>
    /// This is a non-generic TaskCompletionSource<T>
    /// It allows you to set a type via its constructor
    /// </summary>
    public class TaskCompletionSource
    {
        private readonly Type type;
        private readonly object taskCompletionSource;

        public TaskCompletionSource(Type type)
        {
            this.type = type;
            this.taskCompletionSource = typeof(TaskCompletionSource<>)
                        .MakeGenericType(type)
                        .GetConstructors(BindingFlags.Public | BindingFlags.Instance)[0]
                        .Invoke(null);
        }

        public bool TrySetCanceled()
        {
            var trySetCanceled = taskCompletionSource.GetType().GetMethod("TrySetCanceled");
            return (bool)trySetCanceled.Invoke(this.taskCompletionSource, null);
        }

        public bool TrySetException(Exception ex)
        {
            var trySetException = taskCompletionSource.GetType()
                .GetMethods()
                .First(m => m.Name == "TrySetException" && m.GetParameters().First().ParameterType == typeof(IEnumerable<Exception>));
            return (bool)trySetException.Invoke(this.taskCompletionSource, new[] { new[] { ex } });
        }

        public bool TrySetResult(object result)
        {
            var trySetResult = taskCompletionSource.GetType().GetMethod("TrySetResult");
            return (bool)trySetResult.Invoke(this.taskCompletionSource, new[] { result });
        }

        public Task Task
        {
            get
            {
                var taskProperty = taskCompletionSource.GetType().GetProperty("Task");
                return taskProperty.GetValue(this.taskCompletionSource) as Task;
            }
        }
    }
}
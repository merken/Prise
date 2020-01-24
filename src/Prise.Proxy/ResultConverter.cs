using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Prise.Proxy
{
    public abstract class ResultConverter : IResultConverter
    {
        private bool disposed = false;
        public abstract object Deserialize(Type localType, Type remoteType, object value);
        public object ConvertToLocalType(Type localType, Type remoteType, object value)
        {
            return Deserialize(localType, remoteType, value);
        }

        public object ConvertToLocalTypeAsync(Type localType, Type remoteType, Task task)
        {
            var taskResultType = localType.GenericTypeArguments[0];
            var taskCompletionSource = new TaskCompletionSource(taskResultType);

            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                    taskCompletionSource.TrySetCanceled();
                else if (t.IsFaulted)
                    taskCompletionSource.TrySetException(t.Exception);
                else
                {
                    var property = t.GetType()
                                  .GetTypeInfo()
                                  .GetProperties()
                                  .FirstOrDefault(p => p.Name == "Result");

                    if (property != null)
                    {
                        var value = Deserialize(localType, remoteType, property.GetValue(task));
                        taskCompletionSource.TrySetResult(value);
                    }
                }
            });

            return taskCompletionSource.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed && disposing)
            {
                // Nothing to do here
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
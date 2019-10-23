using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace Prise.Infrastructure.NetCore
{
    public class BinaryFormatterResultConverter : IResultConverter
    {
        private bool disposed = false;

        public object ConvertToLocalType(Type localType, object value)
        {
            return DeserializeFromStream(SerializeToStream(value));
        }

        public object ConvertToLocalTypeAsync(Type localType, Task task)
        {
            var taskResultType=localType.GenericTypeArguments[0];
            var taskCompletionSource = new TaskCompletionSource(taskResultType);

            task.ContinueWith(t =>
            {
                if (t.IsCanceled)
                {
                    taskCompletionSource.TrySetCanceled();
                }
                else if (t.IsFaulted)
                {
                    taskCompletionSource.TrySetException(t.Exception);
                }
                else
                {
                    var property = t.GetType()
                                  .GetTypeInfo()
                                  .GetProperties()
                                  .FirstOrDefault(p => p.Name == "Result");

                    if (property != null)
                    {
                        var value = DeserializeFromStream(SerializeToStream(property.GetValue(task)));
                        taskCompletionSource.TrySetResult(value);
                    }
                }
            });

            return taskCompletionSource.Task;
        }

        public static MemoryStream SerializeToStream(object o)
        {
            MemoryStream stream = new MemoryStream();
            IFormatter formatter = new BinaryFormatter();
            formatter.Serialize(stream, o);
            return stream;
        }

        public static object DeserializeFromStream(MemoryStream stream)
        {
            IFormatter formatter = new BinaryFormatter();
            stream.Seek(0, SeekOrigin.Begin);
            object o = formatter.Deserialize(stream);
            return o;
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
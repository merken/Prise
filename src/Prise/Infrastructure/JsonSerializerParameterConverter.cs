using System;
using System.Text.Json;
using Prise.Proxy;

namespace Prise.Infrastructure
{
    public class JsonSerializerParameterConverter : IParameterConverter
    {
        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, localType);
        }

        protected bool disposed = false;
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
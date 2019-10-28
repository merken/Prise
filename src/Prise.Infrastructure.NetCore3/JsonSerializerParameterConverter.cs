using System;
using System.Text.Json;

namespace Prise.Infrastructure.NetCore
{
    public class JsonSerializerParameterConverter : IParameterConverter
    {
        protected bool disposed = false;

        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonSerializer.Serialize(value);
            return JsonSerializer.Deserialize(json, localType);
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
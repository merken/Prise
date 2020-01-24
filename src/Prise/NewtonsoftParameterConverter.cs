#if NETCORE2_1
using System;
using Newtonsoft.Json;
using Prise.Proxy;

namespace Prise
{
    public class NewtonsoftParameterConverter : IParameterConverter
    {
        protected bool disposed = false;

        public object ConvertToRemoteType(Type localType, object value)
        {
            var json = JsonConvert.SerializeObject(value);
            return JsonConvert.DeserializeObject(json, localType);
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
#endif
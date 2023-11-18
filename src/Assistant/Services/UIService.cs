using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Assistant.Services
{
    public class UIService
    {
        public T? FlushObject<T>(T obj,Action<T> action) where T : class
        {
            action.Invoke(obj);
            var tmp = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(tmp);
        }
    }
}

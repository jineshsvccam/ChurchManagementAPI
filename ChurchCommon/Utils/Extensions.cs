using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ChurchCommon.Utils
{
    public static  class Extensions
    {
        public static T Clone<T>(this T source) where T : new()
        {
            if (source == null) throw new ArgumentNullException(nameof(source));

            T clone = new T();
            foreach (PropertyInfo prop in typeof(T).GetProperties())
            {
                if (prop.CanWrite)
                {
                    prop.SetValue(clone, prop.GetValue(source));
                }
            }
            return clone;
        }
        public static T CloneviaJson<T>(this T source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(source));
        }
        public static string Serialize<T>(T obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
        }
    }
}

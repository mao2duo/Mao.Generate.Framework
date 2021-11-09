using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Mao.Core.JsonConverters
{
    /// <summary>
    /// 讓集合轉換成 JSON 時，會以指定的屬性作為 Key 來表示
    /// <para>覆寫 GetKeyProperty 的方法來指定以哪個屬性作為 Key</para>
    /// </summary>
    public abstract class EnumerableToDictionaryConverter<TModel, TKey> : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override bool CanConvert(Type objectType) => typeof(IEnumerable<TModel>).IsAssignableFrom(objectType);

        public abstract PropertyInfo GetKeyProperty();

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var keyProperty = GetKeyProperty();
            var keyValues = serializer.Deserialize<Dictionary<TKey, TModel>>(reader);
            foreach (var keyValue in keyValues)
            {
                keyProperty.SetValue(keyValue.Value, keyValue.Key);
            }
            var enumerable = keyValues.Select(x => x.Value).ToList();
            if (objectType.IsAssignableFrom(typeof(List<TModel>)))
            {
                return enumerable;
            }
            return Activator.CreateInstance(objectType, new object[] { enumerable });
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var keyProperty = GetKeyProperty();
            var sources = value as IEnumerable<TModel>;
            serializer.Serialize(writer, sources.ToDictionary(x => (TKey)keyProperty.GetValue(x), x => x));
        }
    }
}

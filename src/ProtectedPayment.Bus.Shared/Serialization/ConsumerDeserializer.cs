using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace ProtectedPayment.Bus.Shared.Serialization
{
    public class ConsumerDeserializer<T> : IDeserializer<T>
    {
        public T Deserialize(ReadOnlySpan<byte> data, bool isNull, SerializationContext context)
        {
            var json = Encoding.UTF8.GetString(data);

            return JsonConvert.DeserializeObject<T>(json, SerializerSettings.Instance)!;
        }
    }
}

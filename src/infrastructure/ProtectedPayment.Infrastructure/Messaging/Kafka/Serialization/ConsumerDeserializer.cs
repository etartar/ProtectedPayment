using Confluent.Kafka;
using Newtonsoft.Json;
using ProtectedPayment.SharedKernel.Serialization;
using System.Text;

namespace ProtectedPayment.Infrastructure.Messaging.Kafka.Serialization
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

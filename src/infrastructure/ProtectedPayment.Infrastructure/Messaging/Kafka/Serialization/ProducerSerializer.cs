using Confluent.Kafka;
using Newtonsoft.Json;
using ProtectedPayment.SharedKernel.Serialization;
using System.Text;

namespace ProtectedPayment.Infrastructure.Messaging.Kafka.Serialization
{
    public class ProducerSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, SerializerSettings.Instance));
        }
    }
}

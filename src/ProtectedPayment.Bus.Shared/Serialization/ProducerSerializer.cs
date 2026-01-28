using Confluent.Kafka;
using Newtonsoft.Json;
using System.Text;

namespace ProtectedPayment.Bus.Shared.Serialization
{
    public class ProducerSerializer<T> : ISerializer<T>
    {
        public byte[] Serialize(T data, SerializationContext context)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, SerializerSettings.Instance));
        }
    }
}

using ProtectedPayment.Bus.Shared.Enums;

namespace ProtectedPayment.Bus.Shared.Options;

public class ServiceBusOption
{
    public required string RabbitMqConnectionString { get; set; }
    public required string KafkaBootstrapServers { get; set; }
    public BusServiceType BusServiceType { get; set; }
}
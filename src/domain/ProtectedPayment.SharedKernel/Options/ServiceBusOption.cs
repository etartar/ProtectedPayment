namespace ProtectedPayment.SharedKernel.Options;

public class ServiceBusOption
{
    public required string RabbitMqConnectionString { get; set; }
    public required string KafkaBootstrapServers { get; set; }
    public required string RedisBootstrapServers { get; set; }
}
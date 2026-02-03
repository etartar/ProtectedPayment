using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProtectedPayment.Bus.Shared.Abstracts;
using ProtectedPayment.Bus.Shared.Constants;
using ProtectedPayment.Bus.Shared.Events;
using ProtectedPayment.Bus.Shared.Helpers;
using ProtectedPayment.Bus.Shared.Serialization;
using StackExchange.Redis;

namespace ProtectedPayment.Bus.Shared.Services
{
    public sealed class RedisBusService(ILogger<RedisBusService> logger, IRedisConnection redisConnection) : IBusService
    {
        public static Dictionary<object, string> StreamList = new();

        static RedisBusService()
        {
            StreamList.Add(typeof(OrderPendingEvent), RedisBusHelper.GetStreamName<OrderPendingEvent>());
            StreamList.Add(typeof(OrderCancelRequestedEvent), RedisBusHelper.GetStreamName<OrderCancelRequestedEvent>());
            StreamList.Add(typeof(OrderShippedEvent), RedisBusHelper.GetStreamName<OrderShippedEvent>());
        }

        public async Task CreateStreams()
        {
            var db = redisConnection.GetDatabase();

            foreach (var stream in StreamList)
            {
                var streamExists = await db.KeyExistsAsync(stream.Value);

                if (!streamExists)
                {
                    await db.StreamAddAsync(stream.Value, "init", "stream-created");

                    logger.LogInformation("Stream {StreamName} created", stream.Value);
                }
            }
        }

        public async Task PublishAsync<T>(T message, Dictionary<string, object>? headers = null) where T : BaseEvent
        {
            var db = redisConnection.GetDatabase();

            var streamName = StreamList.GetValueOrDefault(message.GetType());

            if (string.IsNullOrEmpty(streamName))
                throw new ArgumentException($"Stream name not found for message type {message.GetType().FullName}");

            var streamPairs = new NameValueEntry[]
            {
                new(QueueConstants.Event, JsonConvert.SerializeObject(message, SerializerSettings.Instance))
            };

            if (headers is not null)
            {
                if (headers.ContainsKey(QueueConstants.MessageKey))
                {
                    string messageKeyValue = headers[QueueConstants.MessageKey].ToString()!;

                    streamPairs = streamPairs.Concat(new NameValueEntry[]
                    {
                        new(QueueConstants.MessageKey, messageKeyValue)
                    }).ToArray();
                }

                foreach (var kvp in headers)
                {
                    var headerValue = kvp.Value?.ToString() ?? string.Empty;

                    streamPairs = streamPairs.Concat(new NameValueEntry[]
                    {
                        new(kvp.Key, headerValue)
                    }).ToArray();
                }
            }

            var messageId = await db.StreamAddAsync(
                key: streamName,
                streamPairs: streamPairs,
                messageId: null,
                maxLength: null,
                useApproximateMaxLength: false);

            logger.LogInformation($"Message sent to stream, messageId {messageId}");
        }
    }
}

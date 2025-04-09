using System.Text;
using RabbitMQ.Client;


namespace NovelExtractor.Messaging
{
    public class ChapterMessageProducer : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private const string ExchangeName = "novel.chapters"; // Must match the producer
        private string _queueName = string.Empty; // Will be assigned by RabbitMQ or set manually
        private string? _consumerTag;

        public ChapterMessageProducer(string hostname = "localhost", string? queueName = null, string bindingKey = "vol.#")
        {
            var factory = new ConnectionFactory() { HostName = hostname };
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;

            // Declare the same topic exchange as the producer (idempotent)
            _channel.ExchangeDeclareAsync(exchange: ExchangeName,
                                     type: ExchangeType.Topic,
                                     durable: true,  // Match producer's setting
                                     autoDelete: false,
                                     arguments: null);

            // Declare a queue
            if (string.IsNullOrEmpty(queueName))
            {
                // Let RabbitMQ generate a unique, non-durable, exclusive, auto-delete queue name
                // Good for temporary consumers (each instance gets its own queue)
                _queueName = _channel.QueueDeclareAsync(queue: "", // Empty name = server generates name
                                                durable: false,
                                                exclusive: true, // Only this connection can use it
                                                autoDelete: true, // Deleted when connection closes
                                                arguments: null).Result.QueueName;
                Console.WriteLine($"[*] Consumer declared temporary queue: '{_queueName}'");
            }
            else
            {
                // Use a specific queue name - potentially shared between consumers
                // Make it durable if you want it to survive broker restarts
                _queueName = queueName;
                _channel.QueueDeclareAsync(queue: _queueName,
                                      durable: true, // Make queue durable
                                      exclusive: false,
                                      autoDelete: false,
                                      arguments: null);
                Console.WriteLine($"[*] Consumer declared durable queue: '{_queueName}'");
            }


            // Bind the queue to the exchange with the specified binding key
            // bindingKey = "vol.#" means "receive all messages starting with 'vol.'"
            // bindingKey = "vol.1.chapter.*" means "receive all chapters for volume 1"
            // bindingKey = "#" means "receive all messages sent to this exchange"
            _channel.QueueBindAsync(queue: _queueName,
                              exchange: ExchangeName,
                              routingKey: bindingKey);

            Console.WriteLine($"[*] Queue '{_queueName}' bound to exchange '{ExchangeName}' with binding key '{bindingKey}'");
        }

        public async Task PublishChapter(int volume, int chapter, string text)
        {
            var routingKey = $"vol.{volume}.chapter.{chapter}";
            var body = Encoding.UTF8.GetBytes(text);

            var props = new BasicProperties();
            props.Persistent = true; // Mark messages as persistent

            await _channel.BasicPublishAsync(
                exchange: ExchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: props,
                body: body
            );

            Console.WriteLine($"[Producer] Published '{routingKey}' ({body.Length} bytes)");
        }


        public ValueTask DisposeAsync()
        {
            _channel.CloseAsync();
            _channel.DisposeAsync();
            _connection.CloseAsync();
            return _connection.DisposeAsync();
        }
    }
}

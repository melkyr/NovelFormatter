using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace NovelExtractor.Messaging // Use a consistent namespace
{
    public class ChapterConsumer : IAsyncDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private const string ExchangeName = "novel.chapters"; // Must match the producer
        private string _queueName = string.Empty;
        private AsyncEventingBasicConsumer? _consumer;
        private string? _consumerTag;

        // Event to notify external code about received messages
        public event EventHandler<(string RoutingKey, string Message)>? MessageReceived;

        // Constructor allows specifying hostname, queue name, and binding key
        public ChapterConsumer(string hostname = "localhost", string? queueName = null, string bindingKey = "vol.#")
        {
            var factory = new ConnectionFactory() { HostName = hostname, ConsumerDispatchConcurrency = 1 }; // Enable async dispatch
            _connection = factory.CreateConnectionAsync().Result;
            _channel = _connection.CreateChannelAsync().Result;
            Console.WriteLine($"[Consumer] Connected to RabbitMQ on '{hostname}'.");


            // Declare the exchange - ensures it exists
            _channel.ExchangeDeclareAsync(exchange: ExchangeName,
                                     type: ExchangeType.Topic,
                                     durable: true, // Match producer
                                     autoDelete: false,
                                     arguments: null);
            Console.WriteLine($"[Consumer] Exchange '{ExchangeName}' declared.");


            // Declare the queue
            if (string.IsNullOrEmpty(queueName))
            {
                // Server-generated, temporary queue
                _queueName = _channel.QueueDeclareAsync(queue: "", durable: false, exclusive: true, autoDelete: true, arguments: null).Result.QueueName;
                Console.WriteLine($"[Consumer] Declared temporary queue: '{_queueName}'. It will be deleted on exit.");
            }
            else
            {
                // Named, durable queue
                _queueName = queueName;
                _channel.QueueDeclareAsync(queue: _queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                Console.WriteLine($"[Consumer] Declared durable queue: '{_queueName}'. It will persist broker restarts.");
            }

            // Bind the queue to the exchange
            _channel.QueueBindAsync(queue: _queueName,
                               exchange: ExchangeName,
                               routingKey: bindingKey);
            Console.WriteLine($"[Consumer] Queue '{_queueName}' bound to exchange '{ExchangeName}' with binding key '{bindingKey}'.");
        }

        public async Task StartConsuming()
        {
            if (_consumer != null)
            {
                Console.WriteLine("[Consumer Warning] Consumer already started.");
                return;
            }

            // Process one message at a time
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);
            Console.WriteLine("[Consumer] Quality of Service set (prefetch count = 1).");


            _consumer = new AsyncEventingBasicConsumer(_channel);

            // Use async event handler if DispatchConsumersAsync = true
            _consumer.ReceivedAsync += async (model, ea) =>
            {
                string message = "";
                string routingKey = "";
                try
                {
                    var body = ea.Body.ToArray();
                    message = Encoding.UTF8.GetString(body);
                    routingKey = ea.RoutingKey;

                    Console.WriteLine($"\n[Consumer] Received message with routing key: '{routingKey}'. Processing...");

                    // --- Simulate Processing ---
                    await Task.Delay(TimeSpan.FromMilliseconds(50)); // Simulate work
                    // Raise the event for external handling
                    OnMessageReceived(routingKey, message);
                    // ------------------------

                    Console.WriteLine($"[Consumer] Processing complete for '{routingKey}'. Acknowledging message.");

                    // Acknowledge message success
                    await _channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[Consumer Error] Failed processing message '{routingKey}': {ex.Message}");
                    Console.ResetColor();
                    // Decide whether to Nack (negative ack) - requeue: true can cause infinite loops if msg always fails
                    await _channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: false); // Send to DLQ/discard if setup
                }
            };

            // Start consuming
            _consumerTag = await _channel.BasicConsumeAsync(queue: _queueName,
                                                 autoAck: false, // Manual acknowledgement
                                                 consumer: _consumer);

            Console.WriteLine($"[*] Consumer started. Listening on queue '{_queueName}'. Consumer Tag: '{_consumerTag}'.");
            Console.WriteLine("[*] Waiting for messages. Press CTRL+C to stop.");

        }

        public async Task StopConsuming()
        {
            if (_consumerTag != null)
            {
                Console.WriteLine($"[Consumer] Cancelling consumer tag '{_consumerTag}'...");
                await _channel.BasicCancelAsync(_consumerTag);
                _consumerTag = null;
                _consumer = null;
                Console.WriteLine("[Consumer] Stopped consuming messages.");
            }
        }

        protected virtual void OnMessageReceived(string routingKey, string message)
        {
            MessageReceived?.Invoke(this, (routingKey, message));
            
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
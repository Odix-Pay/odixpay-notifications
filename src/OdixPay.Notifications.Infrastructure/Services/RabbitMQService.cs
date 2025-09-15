using Microsoft.Extensions.Logging; // Enable logging
using Microsoft.Extensions.Options;
using Polly; // Support retry policies
using RabbitMQ.Client; // RabbitMQ client for messaging
using RabbitMQ.Client.Events;
using System.Text; // Encode message bodies
using OdixPay.Notifications.Domain.Interfaces;
using OdixPay.Notifications.Infrastructure.Configuration;
using OdixPay.Notifications.Domain.Models;
using Newtonsoft.Json;

namespace OdixPay.Notifications.Infrastructure.Services;

/// <summary>
/// Implements event publishing using RabbitMQ as the message broker.
/// Manages connection and channel lifecycle with retry logic and logging.
/// </summary>
public class RabbitMQService : IEventBusService
{
    private readonly IConnection _connection; // RabbitMQ connection
    private readonly IChannel _channel; // RabbitMQ channel for publishing
    private readonly ILogger<IEventBusService> _logger; // Logger for diagnostics
    private readonly string _exchangeName; // Name of the RabbitMQ exchange to publish events
    private const string baseName = "notifications-service"; // Base name for queues
    private bool _disposed; // Tracks disposal state

    /// <summary>
    /// Initializes a new instance of EventPublisherService, setting up RabbitMQ connection and channel.
    /// </summary>
    /// <param name="configuration">Configuration for RabbitMQ settings.</param>
    /// <param name="logger">Logger for tracking operations and errors.</param>
    /// <exception cref="ArgumentNullException">Thrown if configuration or logger is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if RabbitMQ settings are missing.</exception>
    public RabbitMQService(IOptions<RabbitMqConfig> configuration, ILogger<IEventBusService> logger)
    {
        // Validate inputs
        if (configuration == null || configuration.Value == null)
            throw new ArgumentNullException(nameof(configuration));

        var rabbitConfig = configuration.Value;

        _exchangeName = rabbitConfig.ExchangeName ?? throw new InvalidOperationException("Exchange name must be configured in RabbitMqConfig");

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Define retry policy for connection attempts using Polly
        var retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetry(
            retryCount: rabbitConfig.RetryCount,
            sleepDurationProvider: retryAttempt => TimeSpan.FromMilliseconds(rabbitConfig.RetryDelay * retryAttempt),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                _logger.LogWarning($"Failed to connect to RabbitMQ on attempt {retryCount}. Retrying in {timespan.TotalSeconds} seconds.");
            });

        // Execute connection creation with retry
        _connection = retryPolicy.Execute(() =>
        {
            _logger.LogInformation("Attempting to connect to RabbitMQ at {Host}", rabbitConfig.Host);

            var factory = rabbitConfig.GetConnectionFactory();
            var connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            _logger.LogInformation("Successfully connected to RabbitMQ at {Host}", rabbitConfig.Host);
            return connection;

        });

        // Create a channel for publishing messages
        _channel = _connection.CreateChannelAsync().GetAwaiter().GetResult();
        _logger.LogInformation("Created RabbitMQ channel");

        // Declare a topic exchange for event broadcasting. Events must be broadcated to queues (topics)
        _channel.ExchangeDeclareAsync(
            exchange: _exchangeName,
            type: ExchangeType.Topic,
            durable: true, // Persist exchange across broker restarts
            autoDelete: false, // Keep exchange until explicitly deleted
            arguments: null
        ).GetAwaiter().GetResult();

        _logger.LogInformation("Declared fanout exchange: {ExchangeName}", _exchangeName);
    }

    /// <summary>
    /// Publishes an event to RabbitMQ asynchronously.
    /// </summary>
    /// <param name="eventName">Event (queue) to which we want to publish an event.</param>
    /// <param name="data">Data to be published to the event (queue).</param>
    /// <exception cref="ArgumentException">Thrown if eventName is null or empty.</exception>
    public async void Publish<T>(string? eventName, T data, Dictionary<string, string>? headers = null)
    {

        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        // Create event message
        var eventMessage = new MessageEnvelope<T>(data, headers);

        _logger.LogDebug("Publishing event to queue {Queue}", eventName);

        // Serialize message to JSON
        var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventMessage));

        // Publish message to fanout exchange
        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: eventName, // Queue to which the message would be routed to
            mandatory: true,
            body: body
            );

        _logger.LogInformation("Published event {EventName}", eventName);

    }

    /// <summary>
    /// Subscribes to events from RabbitMQ asynchronously.
    /// All messages published by other microservices to this queue (topic) - eventName would be received by this subscriber.
    /// If there is a ReplyTo and CorrelationId from the message being published and the handler returns a value, subscriber will automatically reply to the queue with the returned data of the handler.
    /// </summary>
    /// <typeparam name="T">Type of message to deserialize</typeparam>
    /// <param name="eventName">Event name to subscribe to</param>
    /// <param name="handler">Handler function to process received messages</param>
    /// <exception cref="ArgumentException">Thrown if eventName is null or empty</exception>
    /// <exception cref="ArgumentNullException">Thrown if handler is null</exception>
    public async Task SubscribeAsync<T, TOut>(string eventName, Func<T, Task<TOut?>> handler)
    {
        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        ArgumentNullException.ThrowIfNull(handler);

        try
        {
            // Use a consistent queue name for this service and event
            // This ensures that each service has its own dedicated queue for each event
            // Also ensures that in case of multiple instances of the same service, messages are load-balanced and only one replica processes each message
            var queueName = $"{eventName}.{baseName}";

            await _channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,        // Persist queue across broker restarts
                exclusive: false,     // Allow multiple consumers
                autoDelete: false);   // Don't auto-delete the queue

            // Bind queue to topic exchange with routing key
            await _channel.QueueBindAsync(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: eventName);

            // Set prefetch count to control how many unacknowledged messages a consumer can process
            await _channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false);

            // Set up consumer
            var consumer = new AsyncEventingBasicConsumer(_channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                const int maxRetries = 3;
                var deliveryTag = ea.DeliveryTag;

                try
                {
                    var body = ea.Body.ToArray();
                    var messageText = Encoding.UTF8.GetString(body);

                    _logger.LogDebug("Received message: {Message} with routing key: {RoutingKey}", messageText, ea.RoutingKey);

                    var message = JsonConvert.DeserializeObject<T>(messageText);

                    TOut? result = default;

                    if (message != null)
                    {
                        result = await handler(message);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to deserialize message: {Message}", messageText);
                    }

                    // Respond to RPC if needed
                    var replyTo = ea.BasicProperties?.ReplyTo;
                    var correlationId = ea.BasicProperties?.CorrelationId;

                    // Acknowledge successful processing
                    await _channel.BasicAckAsync(deliveryTag, false);
                    _logger.LogDebug("Message acknowledged successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message");

                    // Get retry count from message headers
                    var headers = (ea.BasicProperties?.Headers as IDictionary<string, object?>) ?? new Dictionary<string, object?>();
                    int retryCount = 0;

                    if (headers.TryGetValue("x-retry-count", out var retryVal))
                    {
                        if (retryVal is byte[] byteVal)
                        {
                            var retryStr = Encoding.UTF8.GetString(byteVal);
                            int.TryParse(retryStr, out retryCount);
                        }
                        else if (retryVal is int intVal)
                        {
                            retryCount = intVal;
                        }
                    }

                    retryCount++;

                    if (retryCount >= maxRetries)
                    {
                        _logger.LogWarning("Message retry limit reached ({RetryCount}), rejecting and sending to DLQ", retryCount);

                        // Reject the message and don't requeue (sends to DLQ if configured)
                        await _channel.BasicNackAsync(deliveryTag, false, requeue: false);
                    }
                    else
                    {
                        _logger.LogInformation("Retrying message, attempt {RetryCount}/{MaxRetries}", retryCount, maxRetries);

                        // Update headers with new retry count
                        headers["x-retry-count"] = retryCount;

                        // Create new properties with updated headers
                        var newProperties = new BasicProperties
                        {
                            Headers = headers,
                            Persistent = true,
                            DeliveryMode = DeliveryModes.Persistent
                        };

                        // Copy other important properties
                        if (ea.BasicProperties != null)
                        {
                            newProperties.MessageId = ea.BasicProperties.MessageId;
                            newProperties.Timestamp = ea.BasicProperties.Timestamp;
                            newProperties.ReplyTo = ea.BasicProperties.ReplyTo;
                            newProperties.CorrelationId = ea.BasicProperties.CorrelationId;
                        }

                        // Republish with delay (optional - you can implement a delay queue)
                        await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)), CancellationToken.None);

                        // Republish the message with updated retry count
                        await _channel.BasicPublishAsync(
                            exchange: _exchangeName,
                            routingKey: ea.RoutingKey,
                            mandatory: true,
                            basicProperties: newProperties,
                            body: ea.Body.ToArray());

                        // Acknowledge the original message since we've republished it
                        await _channel.BasicAckAsync(deliveryTag, false);
                    }
                }
            };


            // Start consuming
            await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer);

            _logger.LogInformation("Subscribed to {EventName} events", eventName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to subscribe to {EventName} events", eventName);
            throw;
        }
    }

    /// <summary>
    /// Sends a request message to RabbitMQ and waits for a response asynchronously using the request-reply pattern.
    /// </summary>
    /// <typeparam name="TRequest">The type of the request message.</typeparam>
    /// <typeparam name="TResponse">The type of the expected response message.</typeparam>
    /// <param name="eventName">The routing key/event name used for message routing.</param>
    /// <param name="request">The request message to be sent.</param>
    /// <param name="timeout">Optional timeout period for the request. Defaults to 10 seconds if not specified.</param>
    /// <returns>A Task containing the response message of type TResponse.</returns>
    /// <exception cref="ArgumentException">Thrown when eventName is null or empty.</exception>
    /// <exception cref="TaskCanceledException">Thrown when the request times out.</exception>
    /// <exception cref="Exception">Thrown when there is an error deserializing the response.</exception>
    public async Task<TResponse> RequestAsync<TRequest, TResponse>(string eventName, TRequest request, TimeSpan? timeout = null)
    {
        if (string.IsNullOrEmpty(eventName))
            throw new ArgumentException("Event name cannot be null or empty", nameof(eventName));

        timeout ??= TimeSpan.FromSeconds(10);

        var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

        var correlationId = Guid.NewGuid().ToString();

        // Declare a temporary exclusive reply queue
        var replyQueue = await _channel.QueueDeclareAsync(
            queue: "",
            durable: false,
            exclusive: true,
            autoDelete: true);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            if (ea.BasicProperties.CorrelationId == correlationId)
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var response = JsonConvert.DeserializeObject<TResponse>(Encoding.UTF8.GetString(body));
                    tcs.TrySetResult(response!);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }
            await Task.Yield();
        };

        // Start consuming the reply queue
        var consumerTag = await _channel.BasicConsumeAsync(
            queue: replyQueue.QueueName,
            autoAck: true,
            consumer: consumer);

        // Prepare request message
        var props = new BasicProperties
        {
            CorrelationId = correlationId,
            ReplyTo = replyQueue.QueueName
        };

        var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request));

        // Publish the request
        await _channel.BasicPublishAsync(
            exchange: _exchangeName,
            routingKey: eventName,
            mandatory: true,
            basicProperties: props,
            body: messageBody
        );

        // Wait for response or timeout
        using var cts = new CancellationTokenSource(timeout.Value);
        await using (cts.Token.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                var response = await tcs.Task;
                return response;
            }
            finally
            {
                // Clean up the consumer
                await _channel.BasicCancelAsync(consumerTag);
            }
        }
    }

    /// <summary>
    /// Disposes of RabbitMQ connection and channel synchronously.
    /// </summary>
    public void Dispose()
    {
        // Run synchronous disposal
        DisposeAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Disposes of RabbitMQ connection and channel asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // Prevent redundant disposal
        if (_disposed)
            return;

        try
        {
            // Close channel if open
            if (_channel?.IsOpen == true)
            {
                _channel.CloseAsync().GetAwaiter().GetResult();
                _logger.LogInformation("Closed RabbitMQ channel");
            }

            // Close connection if open
            if (_connection?.IsOpen == true)
            {
                await _connection.CloseAsync(); // Close is synchronous, wrap in Task
                _logger.LogInformation("Closed RabbitMQ connection");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ resources");
        }
        finally
        {
            _disposed = true;
        }
    }
}
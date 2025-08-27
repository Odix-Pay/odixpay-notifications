namespace OdixPay.Notifications.Domain.Interfaces;

/// <summary>
/// Defines methods for publishing and subscribing to events between microservices.
/// </summary>
public interface IEventBusService : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Publishes an event asynchronously.
    /// </summary>
    /// <typeparam name="T">Type of the event data</typeparam>
    /// <param name="eventName">Name of the event</param>
    /// <param name="message">Event data to publish</param>
    void Publish<T>(string eventName, T message);

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
    Task SubscribeAsync<T, TOut>(string eventName, Func<T, Task<TOut?>> handler);

    /// <summary>
    /// Sends a request message to Topic and waits for a response asynchronously using the request-reply pattern.
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
    Task<TResponse> RequestAsync<TRequest, TResponse>(string eventName, TRequest request, TimeSpan? timeout = null);
}
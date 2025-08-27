namespace OdixPay.Notifications.Application.Interfaces;

public interface IUseCase<TRequest, TResponse>
{
    /// <summary>
    /// Executes the use case with the given request.
    /// </summary>
    /// <param name="request">The request data for the use case.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation, containing the response data.</returns>
    TResponse? ExecuteAsync(TRequest? request, CancellationToken cancellationToken = default);
}
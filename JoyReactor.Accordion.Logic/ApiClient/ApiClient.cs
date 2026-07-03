using GraphQL;
using GraphQL.Client.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace JoyReactor.Accordion.Logic.ApiClient;

public class ApiClient(
    IGraphQLClient graphQlClient,
    IOptions<ApiClientSettings> settings,
    ILogger<ApiClient> logger)
    : IApiClient
{
    protected readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    protected readonly ResiliencePipeline ResiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<TimeoutRejectedException>()
                .Handle<HttpRequestException>()
                .Handle<GraphQL.Client.Http.GraphQLHttpRequestException>(),
            MaxRetryAttempts = settings.Value.MaxRetryAttempts,
            Delay = settings.Value.RetryDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                logger.LogWarning("Failed to send GraphQL request to API. Attempt: {Attempt}/{MaxAttempts}. Message: {ExceptionMessage}.", args.AttemptNumber + 1, settings.Value.MaxRetryAttempts, args.Outcome.Exception?.Message);
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromSeconds(15))
        .Build();

    public async Task<T> SendAsync<T>(GraphQLRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await Semaphore.WaitAsync(cancellationToken);
            await Task.Delay(settings.Value.SubsequentCallDelay, cancellationToken);

            return await ResiliencePipeline.ExecuteAsync(async ct =>
            {
                var response = await graphQlClient.SendQueryAsync<T>(request, ct);
                foreach (var error in response.Errors ?? [])
                    logger.LogError("Failed response from GraphQL API recieved. Message: {ExceptionMessage}.", error.Message);

                return response.Data;
            }, cancellationToken);
        }
        finally
        {
            Semaphore.Release();
        }
    }
}

public interface IApiClient
{
    Task<T> SendAsync<T>(GraphQLRequest request, CancellationToken cancellationToken);
}
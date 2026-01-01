using GraphQL;
using GraphQL.Client.Abstractions;
using JoyReactor.Accordion.Logic.ApiClient.Responses;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace JoyReactor.Accordion.Logic.ApiClient;

public class ApiClient(
    IGraphQLClient graphQlClient,
    IOptions<ApiClientSettings> settings,
    ILogger<ApiClient> logger)
    : IApiClient
{
    internal static readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

    internal readonly ResiliencePipeline resiliencePipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder()
                .Handle<HttpRequestException>()
                .Handle<GraphQL.Client.Http.GraphQLHttpRequestException>(),
            MaxRetryAttempts = settings.Value.MaxRetryAttempts,
            Delay = settings.Value.SubsequentCallDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                logger.LogWarning("Failed to send GraphQL request to API. Message: {Message}. Attempt: {Attempt}", args.Outcome.Exception?.Message, args.AttemptNumber);
                return default;
            }
        })
        .AddTimeout(TimeSpan.FromSeconds(10))
        .Build();

    public async Task<T> SendAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default)
    {
        await semaphore.WaitAsync(cancellationToken);

        try
        {
            await Task.Delay(settings.Value.SubsequentCallDelay);

            return await resiliencePipeline.ExecuteAsync(async ct =>
            {
                var response = await graphQlClient.SendQueryAsync<T>(request, ct);
                foreach (var error in response.Errors ?? [])
                    logger.LogError("Failed response from GraphQL API recieved. Message: {Message}", error.Message);

                return response.Data;
            }, cancellationToken);
        }
        catch
        {
            throw;
        }
        finally
        {
            semaphore.Release();
        }
    }
}

public interface IApiClient
{
    Task<T> SendAsync<T>(GraphQLRequest request, CancellationToken cancellationToken = default);
}
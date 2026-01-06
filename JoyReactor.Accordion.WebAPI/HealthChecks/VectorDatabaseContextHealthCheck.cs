using Microsoft.Extensions.Diagnostics.HealthChecks;
using Qdrant.Client;

namespace JoyReactor.Accordion.WebAPI.HealthChecks;

public class VectorDatabaseContextHealthCheck(
    IQdrantClient qdrantClient,
    ILogger<VectorDatabaseContextHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await qdrantClient.HealthAsync(cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Health check failed. Message: {ExceptionMessage}.", ex.Message);
            return HealthCheckResult.Unhealthy();
        }
    }
}
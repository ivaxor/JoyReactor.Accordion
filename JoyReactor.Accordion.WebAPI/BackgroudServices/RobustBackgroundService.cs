
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.Extensions.Options;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public abstract class RobustBackgroundService(
    IOptions<BackgroundServiceSettings> settings,
    ILogger<RobustBackgroundService> logger)
    : BackgroundService
{
    protected readonly PeriodicTimer PeriodicTimer = new PeriodicTimer(settings.Value.SubsequentRunDelay);
    protected abstract bool IsIndefinite { get; }
    protected bool IsDisabled => settings.Value.DisabledServiceNames.Contains(GetType().Name, StringComparer.OrdinalIgnoreCase);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        do
        {
            if (IsDisabled)
                return;

            using (logger.BeginScope(new Dictionary<string, object>() { { "TraceId", Guid.NewGuid() } }))
            {
                try
                {
                    await RunAsync(cancellationToken);
                    if (IsIndefinite)
                    {
                        logger.LogInformation("{BackgroundServiceName} background service succesfully ran. Next run is scheduled.", GetType().Name);
                    }
                    else
                    {
                        logger.LogInformation("{BackgroundServiceName} background service succesfully ran.", GetType().Name);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "{BackgroundServiceName} background service failed. Next run is scheduled.", GetType().Name);
                }
            }
        } while (await PeriodicTimer.WaitForNextTickAsync(cancellationToken));
    }

    protected abstract Task RunAsync(CancellationToken cancellationToken);
}
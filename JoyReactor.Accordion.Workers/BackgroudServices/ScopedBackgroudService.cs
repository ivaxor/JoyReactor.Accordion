using JoyReactor.Accordion.Workers.Extensions;
using Microsoft.Extensions.Hosting;

namespace JoyReactor.Accordion.Workers.BackgroudServices;

public abstract class ScopedBackgroudService : BackgroundService
{
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (BackgroundServiceExtensions.ServiceScopes.TryRemove(GetType(), out var serviceScope))
            serviceScope.Dispose();
    }

    public override void Dispose()
    {
        if (BackgroundServiceExtensions.ServiceScopes.TryRemove(GetType(), out var serviceScope))
            serviceScope.Dispose();

        base.Dispose();
    }
}
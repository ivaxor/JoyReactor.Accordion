using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class VectorPostDuplicateRemover(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<QdrantSettings> qdrantSettings,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<VectorNormalizator> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        var qdrantClient = serviceScope.ServiceProvider.GetRequiredService<IQdrantClient>();

        var knownPostAttributeIdPairs = new HashSet<(int postId, int postAttributeId)>();

        var scrollOffset = (PointId)null;
        var scrollResponse = (ScrollResponse)null;
        do
        {
            var duplicatePointIds = new List<PointId>();

            scrollResponse = await qdrantClient.ScrollAsync(
                collectionName: qdrantSettings.Value.CollectionName,
                limit: 10000,
                filter: new Filter
                {
                    Must = {
                        new Condition { Field = new FieldCondition() { Key = "postId", IsEmpty = false } },
                        new Condition { Field = new FieldCondition() { Key = "postAttributeId", IsEmpty = false } },
                    }
                },
                offset: scrollOffset,
                vectorsSelector: false,
                payloadSelector: true,
                cancellationToken: cancellationToken);
            scrollOffset = scrollResponse.NextPageOffset;

            logger.LogInformation("Checking {VectorCount} vector(s) for duplicates.", scrollResponse.Result.Count);
            foreach (var scrollPoint in scrollResponse.Result)
            {
                var postId = Convert.ToInt32(scrollPoint.Payload["postId"].IntegerValue);
                var postAttributeId = Convert.ToInt32(scrollPoint.Payload["postAttributeId"].IntegerValue);
                if (!knownPostAttributeIdPairs.Add((postId, postAttributeId)))
                {
                    duplicatePointIds.Add(scrollPoint.Id);
                    logger.LogInformation("Found duplicated vector for {PostAttributeNumberId} attribute of {PostNumberId} post.", postAttributeId, postId);
                }
            }

            if (duplicatePointIds.Count == 0)
                continue;

            await qdrantClient.DeleteAsync(
                collectionName: qdrantSettings.Value.CollectionName,
                ids: duplicatePointIds,
                cancellationToken: cancellationToken);
            logger.LogInformation("Delete {VectorCount} duplicate vector(s).", duplicatePointIds.Count);
        } while (scrollResponse.Result.Count != 0 && scrollResponse.NextPageOffset != null);
    }
}
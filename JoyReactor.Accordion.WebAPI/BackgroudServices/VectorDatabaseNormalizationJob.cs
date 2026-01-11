using JoyReactor.Accordion.Logic.Database.Vector;
using JoyReactor.Accordion.WebAPI.Models;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public class VectorDatabaseNormalizationJob(
    IServiceScopeFactory serviceScopeFactory,
    IOptions<QdrantSettings> qdrantSettings,
    IOptions<BackgroundServiceSettings> settings,
    ILogger<VectorDatabaseNormalizationJob> logger)
    : RobustBackgroundService(settings, logger)
{
    protected override bool IsIndefinite => true;

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await using var serviceScope = serviceScopeFactory.CreateAsyncScope();
        var qdrantClient = serviceScope.ServiceProvider.GetRequiredService<IQdrantClient>();

        var scrollOffset = (PointId)null;
        var scrollResponse = (ScrollResponse)null;
        do
        {
            var updatedPoints = new List<PointStruct>();
            scrollResponse = await qdrantClient.ScrollAsync(
                collectionName: qdrantSettings.Value.CollectionName,
                limit: 1000,
                filter: new Filter
                {
                    Should = {
                        new Condition { Field = new FieldCondition() { Key = "postIds", IsEmpty = false } },
                        new Condition { Field = new FieldCondition() { Key = "attributeIds", IsEmpty = false } },
                    }
                },
                offset: scrollOffset,
                vectorsSelector: false,
                payloadSelector: true,
                cancellationToken: cancellationToken);
            scrollOffset = scrollResponse.NextPageOffset;

            foreach (var scrollPoint in scrollResponse.Result)
            {
                var isUpdated = false;
                if (scrollPoint.Payload.TryGetValue("postIds", out var postIdsValue) && postIdsValue.KindCase == Value.KindOneofCase.ListValue)
                {
                    scrollPoint.Payload["postId"] = new Value() { IntegerValue = int.Parse(postIdsValue.ListValue.Values.Single().StringValue) };
                    scrollPoint.Payload.Remove("postIds");
                    isUpdated = true;
                }

                if (scrollPoint.Payload.TryGetValue("attributeIds", out var postAttributeIds) && postAttributeIds.KindCase == Value.KindOneofCase.ListValue)
                {
                    scrollPoint.Payload["postAttributeId"] = new Value() { IntegerValue = int.Parse(postAttributeIds.ListValue.Values.Single().StringValue) };
                    scrollPoint.Payload.Remove("attributeIds");
                    isUpdated = true;
                }

                if (!isUpdated)
                    continue;

                var pointStruct = new PointStruct();
                pointStruct.Id = scrollPoint.Id;
                pointStruct.Payload.Add(scrollPoint.Payload);
                updatedPoints.Add(pointStruct);
            }

            if (updatedPoints.Count == 0)
                continue;

            await qdrantClient.UpsertAsync(
                collectionName: qdrantSettings.Value.CollectionName,
                points: updatedPoints,
                cancellationToken: cancellationToken);
            logger.LogInformation("Normalized {VectorCount} vector payloads.", updatedPoints.Count);
        } while (scrollResponse.Result.Count != 0 && scrollResponse.NextPageOffset != null);
    }
}
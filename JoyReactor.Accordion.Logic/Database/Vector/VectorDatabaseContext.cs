using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Globalization;

namespace JoyReactor.Accordion.Logic.Database.Vector;

public class VectorDatabaseContext(
    IQdrantClient qdrantClient,
    IOptions<QdrantSettings> settings)
    : IVectorDatabaseContext
{
    public async Task UpsertAsync(ParsedPostAttributePicture picture, float[] vector, CancellationToken cancellationToken)
    {
        var point = CreatePointStruct(picture, vector);
        await qdrantClient.UpsertAsync(settings.Value.CollectionName, [point], cancellationToken: cancellationToken);
    }

    public async Task UpsertAsync(IDictionary<ParsedPostAttributePicture, float[]> pictureVectors, CancellationToken cancellationToken)
    {
        var points = pictureVectors
            .Select(kvp => CreatePointStruct(kvp.Key, kvp.Value))
            .ToArray();

        await qdrantClient.UpsertAsync(settings.Value.CollectionName, points, cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<ScoredPoint>> SearchRawAsync(float[] vector, CancellationToken cancellationToken)
    {
        return await qdrantClient.SearchAsync(
            settings.Value.CollectionName,
            vector,
            limit: settings.Value.SearchLimit,
            scoreThreshold: settings.Value.SearchScoreThreshold,
            cancellationToken: cancellationToken);
    }

    public async Task<PictureScoredPoint[]> SearchAsync(float[] vector, CancellationToken cancellationToken)
    {
        var results = await SearchRawAsync(vector, cancellationToken);

        return results
            .Select(result => new PictureScoredPoint(result))
            .ToArray();
    }

    public async Task<ulong> CountAsync(CancellationToken cancellationToken)
    {
        return await qdrantClient.CountAsync(settings.Value.CollectionName, cancellationToken: cancellationToken);
    }

    public async Task<ScrollResponse> ScrollAsync(PointId? offset, bool includeVectors, bool includePayload, CancellationToken cancellationToken)
    {
        return await qdrantClient.ScrollAsync(
            collectionName: settings.Value.CollectionName,
            limit: 100,
            offset: offset,
            vectorsSelector: includeVectors,
            payloadSelector: includePayload,
            cancellationToken: cancellationToken);
    }

    protected static PointStruct CreatePointStruct(ParsedPostAttributePicture picture, float[] vector)
    {
        return new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = vector,
            Payload = {
                ["postId"] = new Value() { IntegerValue =  picture.PostId.ToInt() },
                ["postAttributeId"] = new Value() { IntegerValue = picture.AttributeId },
                ["createdAt"] = new Value() { StringValue = picture.CreatedAt.ToString("o", CultureInfo.InvariantCulture) },
            },
        };
    }
}

public interface IVectorDatabaseContext
{
    Task UpsertAsync(ParsedPostAttributePicture picture, float[] vector, CancellationToken cancellationToken);
    Task UpsertAsync(IDictionary<ParsedPostAttributePicture, float[]> pictureVectors, CancellationToken cancellationToken);
    Task<IReadOnlyList<ScoredPoint>> SearchRawAsync(float[] vector, CancellationToken cancellationToken);
    Task<PictureScoredPoint[]> SearchAsync(float[] vector, CancellationToken cancellationToken);
    Task<ulong> CountAsync(CancellationToken cancellationToken);
    Task<ScrollResponse> ScrollAsync(PointId? offset, bool includeVectors, bool includePayload, CancellationToken cancellationToken);
}
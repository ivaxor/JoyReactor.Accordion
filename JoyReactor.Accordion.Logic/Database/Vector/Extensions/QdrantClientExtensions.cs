using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Globalization;

namespace JoyReactor.Accordion.Logic.Database.Vector.Extensions;

public static class QdrantClientExtensions
{
    public static async Task UpsertAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        ParsedPostAttributePicture picture, float[] vector,
        CancellationToken cancellationToken)
    {
        var point = CreatePointStruct(picture, vector);
        await qdrantClient.UpsertAsync(collectionName, [point], cancellationToken: cancellationToken);
    }

    public static async Task UpsertAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        IDictionary<ParsedPostAttributePicture, float[]> pictureVectors,
        CancellationToken cancellationToken)
    {
        var points = pictureVectors
            .Select(kvp => CreatePointStruct(kvp.Key, kvp.Value))
            .ToArray();

        await qdrantClient.UpsertAsync(collectionName, points, cancellationToken: cancellationToken);
    }

    public static async Task<IReadOnlyList<ScoredPoint>> SearchRawAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        int limit,
        float scoreThreshold,
        float[] vector,
        CancellationToken cancellationToken)
    {
        return await qdrantClient.SearchAsync(
            collectionName,
            vector,
            limit: Convert.ToUInt64(limit),
            scoreThreshold: scoreThreshold,
            cancellationToken: cancellationToken);
    }

    public static async Task<PictureScoredPoint[]> SearchAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        int limit,
        float scoreThreshold,
        float[] vector,
        CancellationToken cancellationToken)
    {
        var results = await SearchRawAsync(qdrantClient, collectionName, limit, scoreThreshold, vector, cancellationToken);

        return results
            .Select(result => new PictureScoredPoint(result))
            .ToArray();
    }

    public static async Task<ulong> CountAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        CancellationToken cancellationToken)
    {
        return await qdrantClient.CountAsync(collectionName, cancellationToken: cancellationToken);
    }

    public static async Task<ScrollResponse> ScrollAsync(
        this IQdrantClient qdrantClient,
        string collectionName,
        int limit,
        PointId? offset,
        bool includeVectors,
        bool includePayload, CancellationToken cancellationToken)
    {
        return await qdrantClient.ScrollAsync(
            collectionName: collectionName,
            limit: Convert.ToUInt32(limit),
            offset: offset,
            vectorsSelector: includeVectors,
            payloadSelector: includePayload,
            cancellationToken: cancellationToken);
    }

    private static PointStruct CreatePointStruct(ParsedPostAttributePicture picture, float[] vector)
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
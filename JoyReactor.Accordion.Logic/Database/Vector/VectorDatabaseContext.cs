using JoyReactor.Accordion.Logic.Database.Sql.Entities;
using JoyReactor.Accordion.Logic.Database.Vector.Entities;
using JoyReactor.Accordion.Logic.Extensions;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

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

    public async Task<ImagePayload[]> SearchAsync(float[] vector, CancellationToken cancellationToken)
    {
        var results = await qdrantClient.SearchAsync(
            settings.Value.CollectionName,
            vector,
            limit: settings.Value.SearchLimit,
            scoreThreshold: settings.Value.SearchScoreThreshold,
            cancellationToken: cancellationToken);

        return results
            .Select(result => new ImagePayload(result))
            .ToArray();
    }

    internal static PointStruct CreatePointStruct(ParsedPostAttributePicture picture, float[] vector)
    {
        return new PointStruct
        {
            Id = Guid.NewGuid(),
            Vectors = vector,
            Payload = {
                ["postIds"] = new string[] { picture.PostId.ToInt().ToString() },
                ["attributeIds"] = new string[] { picture.AttributeId.ToString() },
            },
        };
    }
}

public interface IVectorDatabaseContext
{
    Task UpsertAsync(ParsedPostAttributePicture picture, float[] vector, CancellationToken cancellationToken);
    Task UpsertAsync(IDictionary<ParsedPostAttributePicture, float[]> pictureVectors, CancellationToken cancellationToken);
    Task<ImagePayload[]> SearchAsync(float[] vector, CancellationToken cancellationToken);
}
using Qdrant.Client.Grpc;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.Database.Vector.Entities;

public record ImagePayload
{
    public ImagePayload() { }

    public ImagePayload(ScoredPoint scoredPoint)
    {
        PostIds = scoredPoint.Payload.TryGetValue("postIds", out var postIdsValue) && postIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? postIdsValue.ListValue.Values.Select(v => v.StringValue).ToHashSet(StringComparer.Ordinal)
            : [];

        AttributeIds = scoredPoint.Payload.TryGetValue("attributeIds", out var attributeIdsValue) && attributeIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? attributeIdsValue.ListValue.Values.Select(v => v.StringValue).ToHashSet(StringComparer.Ordinal)
            : [];
    }

    [JsonPropertyName("postIds")]
    public HashSet<string> PostIds { get; set; }

    [JsonPropertyName("attributeIds")]
    public HashSet<string> AttributeIds { get; set; }
}
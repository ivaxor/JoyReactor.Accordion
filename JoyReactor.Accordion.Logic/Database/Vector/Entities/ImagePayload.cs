using Qdrant.Client.Grpc;

namespace JoyReactor.Accordion.Logic.Database.Vector.Entities;

public record VectorSearchResult
{
    public VectorSearchResult() { }

    public VectorSearchResult(ScoredPoint scoredPoint)
    {
        Score = scoredPoint.Score;

        PostIds = scoredPoint.Payload.TryGetValue("postIds", out var postIdsValue) && postIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? postIdsValue.ListValue.Values.Select(v => v.StringValue).ToHashSet(StringComparer.Ordinal)
            : [];

        AttributeIds = scoredPoint.Payload.TryGetValue("attributeIds", out var attributeIdsValue) && attributeIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? attributeIdsValue.ListValue.Values.Select(v => v.StringValue).ToHashSet(StringComparer.Ordinal)
            : [];
    }

    public float Score { get; set; }
    public HashSet<string> PostIds { get; set; }
    public HashSet<string> AttributeIds { get; set; }
}
using Qdrant.Client.Grpc;

namespace JoyReactor.Accordion.Logic.Database.Vector.Entities;

public record PictureScoredPoint
{
    public PictureScoredPoint() { }

    public PictureScoredPoint(ScoredPoint scoredPoint)
    {
        Score = scoredPoint.Score;

        PostIds = scoredPoint.Payload.TryGetValue("postIds", out var postIdsValue) && postIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? postIdsValue.ListValue.Values.Select(v => v.StringValue).Select(v => int.Parse(v)).ToArray()
            : [];

        PostAttributeIds = scoredPoint.Payload.TryGetValue("attributeIds", out var attributeIdsValue) && attributeIdsValue.KindCase == Value.KindOneofCase.ListValue
            ? attributeIdsValue.ListValue.Values.Select(v => v.StringValue).Select(v => int.Parse(v)).ToArray()
            : [];
    }

    public float Score { get; set; }
    public int[] PostIds { get; set; }
    public int[] PostAttributeIds { get; set; }
}
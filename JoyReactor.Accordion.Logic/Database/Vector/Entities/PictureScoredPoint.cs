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

        PostId = scoredPoint.Payload.TryGetValue("postId", out var postIdValue) && postIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(postIdValue.IntegerValue)
            : null;

        PostAttributeId = scoredPoint.Payload.TryGetValue("postAttributeId", out var postAttributeId) && postAttributeId.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(postAttributeId.IntegerValue)
            : null;

        CommentId = scoredPoint.Payload.TryGetValue("commentId", out var commentIdValue) && postIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(postIdValue.IntegerValue)
            : null;

        CommentAttributeId = scoredPoint.Payload.TryGetValue("commentAttributeId", out var commentAttributeIdValue) && commentAttributeIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(commentAttributeIdValue.IntegerValue)
            : null;
    }

    public float Score { get; set; }

    [Obsolete("Use PostId instead")]
    public int[] PostIds { get; set; }
    [Obsolete("Use PostAttributeId instead")]
    public int[] PostAttributeIds { get; set; }

    public int? PostId { get; set; }
    public int? PostAttributeId { get; set; }

    public int? CommentId { get; set; }
    public int? CommentAttributeId { get; set; }
}
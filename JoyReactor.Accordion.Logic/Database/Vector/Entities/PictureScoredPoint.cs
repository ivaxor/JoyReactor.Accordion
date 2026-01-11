using Qdrant.Client.Grpc;

namespace JoyReactor.Accordion.Logic.Database.Vector.Entities;

public record PictureScoredPoint
{
    public PictureScoredPoint() { }

    public PictureScoredPoint(ScoredPoint scoredPoint)
    {
        Score = scoredPoint.Score;

        PostId = scoredPoint.Payload.TryGetValue("postId", out var postIdValue) && postIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(postIdValue.IntegerValue)
            : null;

        PostAttributeId = scoredPoint.Payload.TryGetValue("postAttributeId", out var postAttributeId) && postAttributeId.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(postAttributeId.IntegerValue)
            : null;

        CommentId = scoredPoint.Payload.TryGetValue("commentId", out var commentIdValue) && commentIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(commentIdValue.IntegerValue)
            : null;

        CommentAttributeId = scoredPoint.Payload.TryGetValue("commentAttributeId", out var commentAttributeIdValue) && commentAttributeIdValue.KindCase == Value.KindOneofCase.IntegerValue
            ? Convert.ToInt32(commentAttributeIdValue.IntegerValue)
            : null;
    }

    public float Score { get; set; }

    public int? PostId { get; set; }
    public int? PostAttributeId { get; set; }

    public int? CommentId { get; set; }
    public int? CommentAttributeId { get; set; }
}
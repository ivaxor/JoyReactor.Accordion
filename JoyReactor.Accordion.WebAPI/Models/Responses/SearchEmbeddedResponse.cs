using JoyReactor.Accordion.Logic.Extensions;

namespace JoyReactor.Accordion.WebAPI.Models.Responses;

public record SearchEmbeddedResponse
{
    public SearchEmbeddedResponse() { }
    public SearchEmbeddedResponse(IEnumerable<Guid> postIds)
    {
        PostIds = postIds.Select(postId => postId.ToInt()).ToArray();
    }

    public int[] PostIds { get; set; }
}
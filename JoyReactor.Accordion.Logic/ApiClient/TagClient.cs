using GraphQL;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Collections.Frozen;
using System.Text;

namespace JoyReactor.Accordion.Logic.ApiClient;

public class TagClient(IApiClient apiClient)
    : ITagClient
{
    protected static readonly FrozenDictionary<TagLineType, string> TagLineTypeToValue = new Dictionary<TagLineType, string>()
    {
        { TagLineType.NEW, "NEW" },
        { TagLineType.BEST, "BEST" },
    }.ToFrozenDictionary();

    public async Task<Tag> GetAsync(int numberId, TagLineType type, CancellationToken cancellationToken)
    {
        const string query = @"
query TagClient_GetAsync($nodeId: ID!, $type: TagLineType!) {
  node(id: $nodeId) {
    ... on Tag {
      id
      name
      count
      subscribers
      mainTag {
        id
        name
      }
      hierarchy {
        id
        name
      }
      tagPager(type: $type) {
        count
      }
    }
  }
}";

        var nodeId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"Tag:{numberId}"));
        var request = new GraphQLRequest(query, new { nodeId, type = TagLineTypeToValue[type] });
        var response = await apiClient.SendAsync<ApiClientNodeResponse<Tag>>(request, cancellationToken);
        return response.Node;
    }

    public async Task<Tag> GetByNameAsync(string name, TagLineType lineType, CancellationToken cancellationToken)
    {
        const string query = @"
query TagClient_GetByNameAsync($name: String!, $type: TagLineType!) {
  node: tag(name: $name) {
    ... on Tag {
      id
      name
      count
      subscribers
      mainTag {
        id
        name
      }
      hierarchy {
        id
        name
      }
      tagPager(type: $type) {
        count
      }
    }
  }
}";

        var type = TagLineTypeToValue[lineType];
        var request = new GraphQLRequest(query, new { name, type });
        var response = await apiClient.SendAsync<ApiClientNodeResponse<Tag>>(request, cancellationToken);
        return response.Node;
    }

    public async Task<TagPager> GetSubTagsAsync(int parentNumberId, TagLineType lineType, int page, CancellationToken cancellationToken)
    {
        const string query = @"
query TagClient_GetSubTagsAsync($nodeId: ID!, $page: Int!, $type: TagLineType!) {
  node(id: $nodeId) {
    ... on TagPager {
      id
      count
      tags(page: $page) {
        ... on Tag {
          id
          name
          count
          subscribers
          mainTag {
            id
            name
          }
          hierarchy {
            id
            name
          }
          tagPager(type: $type) {
            count
          }
        }
      }
    }
  }
}";

        var type = TagLineTypeToValue[lineType];
        var nodeId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"TagPager:Category,{parentNumberId},{type}"));
        var request = new GraphQLRequest(query, new { nodeId, page, type });
        var response = await apiClient.SendAsync<ApiClientNodeResponse<TagPager>>(request, cancellationToken);
        return response.Node;
    }

    public async Task<Tag[]> GetAllSubTagsAsync(int parentNumberId, TagLineType lineType, CancellationToken cancellationToken)
    {
        var page = 0;
        var tagPager = (TagPager)null;
        var subTags = new List<Tag>();

        do
        {
            page++;

            tagPager = await GetSubTagsAsync(parentNumberId, lineType, page, cancellationToken);
            subTags.AddRange(tagPager.Tags);
        } while (subTags.Count() < tagPager.TotalCount);

        return subTags.ToArray();
    }
}

public interface ITagClient
{
    Task<Tag> GetAsync(int numberId, TagLineType type, CancellationToken cancellationToken);
    Task<Tag> GetByNameAsync(string name, TagLineType lineType, CancellationToken cancellationToken);
    Task<TagPager> GetSubTagsAsync(int parentNumberId, TagLineType lineType, int page, CancellationToken cancellationToken);
    Task<Tag[]> GetAllSubTagsAsync(int parentNumberId, TagLineType lineType, CancellationToken cancellationToken);
}
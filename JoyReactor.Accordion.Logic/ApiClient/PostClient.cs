using GraphQL;
using JoyReactor.Accordion.Logic.ApiClient.Models;
using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Collections.Frozen;
using System.Text;

namespace JoyReactor.Accordion.Logic.ApiClient;

public class PostClient(IApiClient apiClient)
    : IPostClient
{
    internal static readonly FrozenDictionary<PostLineType, int> PostLineTypeToValue = new Dictionary<PostLineType, int>() {
        { PostLineType.ALL, 0 },
        { PostLineType.GOOD, 1 },
        { PostLineType.BEST, 2 },
        { PostLineType.NEW, 5 },
    }.ToFrozenDictionary();

    public async Task<Post> GetAsync(int numberId, CancellationToken cancellationToken = default)
    {
        const string query = @"
query PostClient_GetAsync($nodeId: ID!) {
  node(id: $nodeId) {
    ... on Post {
      id
      contentVersion
      attributes {
        type
        ... on PostAttributePicture {
          id
          image {
            id
            type
          }
        }
        ... on PostAttributeEmbed {
          value
        }
      }
    }
  }
}";

        var nodeId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"Post:{numberId}"));
        var request = new GraphQLRequest(query, new { nodeId });
        var response = await apiClient.SendAsync<ApiClientNodeResponse<Post>>(request, cancellationToken);
        return response.Node;
    }

    public async Task<PostPager> GetByTagAsync(int tagNumberId, PostLineType lineType, int page, CancellationToken cancellationToken = default)
    {
        const string query = @"
query PostClient_GetByTagAsync($nodeId: ID!, $page: Int) {
  node(id: $nodeId) {
    ... on PostPager {
      id
      count
      posts(page: $page, offset: 0) {
        ... on Post {
          id
          contentVersion
          attributes {
            type
            ... on PostAttributePicture {
              id
              image {
                id
                type
              }
            }
            ... on PostAttributeEmbed {
              value
            }
          }
        }
      }
    }
  }
}";

        var type = PostLineTypeToValue[lineType];
        var nodeId = Convert.ToBase64String(Encoding.UTF8.GetBytes($"PostPager:Tag,{tagNumberId},{type},{type}"));
        var request = new GraphQLRequest(query, new { nodeId, page });
        var response = await apiClient.SendAsync<ApiClientNodeResponse<PostPager>>(request, cancellationToken);
        return response.Node;
    }

    public async Task<Post[]> GetWeekTopPostsAsync(int year, int week, bool nsfw, CancellationToken cancellationToken = default)
    {
        const string query = @"
query PostClient_GetWeekTopPostsAsync($year:Int!, $week: Int!, $nsfw: Boolean!) {
  weekTopPosts(year: $year, week: $week, nsfw: $nsfw) {
... on Post {
          id
          contentVersion
          attributes {
            type
            ... on PostAttributePicture {
              id
              image {
                id
                type
              }
            }
            ... on PostAttributeEmbed {
              value
            }
          }
        }
  }
}";

        var request = new GraphQLRequest(query, new { year, week, nsfw });
        var response = await apiClient.SendAsync<ApiClientWeekTopPostsResponse>(request, cancellationToken);
        return response.Posts;
    }
}

public interface IPostClient
{
    Task<Post> GetAsync(int numberId, CancellationToken cancellationToken = default);
    Task<PostPager> GetByTagAsync(int tagNumberId, PostLineType type, int page, CancellationToken cancellationToken = default);
    Task<Post[]> GetWeekTopPostsAsync(int year, int week, bool nsfw, CancellationToken cancellationToken = default);
}
using JoyReactor.Accordion.Logic.ApiClient.Models;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Responses;

public record ApiClientWeekTopPostsResponse
{
    [JsonPropertyName("weekTopPosts")]
    public Post[] Posts { get; set; }
}
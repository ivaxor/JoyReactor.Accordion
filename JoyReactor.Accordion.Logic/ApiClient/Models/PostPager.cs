using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record PostPager : NodeResponseObject
{
    [JsonPropertyName("count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("posts")]
    public Post[] Posts { get; set; }
}
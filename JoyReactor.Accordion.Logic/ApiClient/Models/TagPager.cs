using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record TagPager : NodeResponseObject
{
    [JsonPropertyName("count")]
    public int? TotalCount { get; set; }

    [JsonPropertyName("tags")]
    public Tag[]? Tags { get; set; }
}
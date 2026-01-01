using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record Tag : NodeResponseObject
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("count")]
    public int? PostCount { get; set; }

    [JsonPropertyName("subscribers")]
    public int? SubscriberCount { get; set; }

    [JsonPropertyName("mainTag")]
    public Tag? MainTag { get; set; }

    [JsonPropertyName("hierarchy")]
    public Tag[]? Hierarchy { get; set; }

    [JsonPropertyName("tagPager")]
    public TagPager? Pager { get; set; }
}
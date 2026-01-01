using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record Post : NodeResponseObject
{
    [JsonPropertyName("contentVersion")]
    public int? ContentVersion { get; set; }

    [JsonPropertyName("attributes")]
    public PostAttribute[]? Attributes { get; set; }
}
using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record Image : NodeResponseObject
{
    /// <summary>
    /// Possible value:
    /// PNG, JPEG, GIF, BMP, TIFF,
    /// MP4, WEBM
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
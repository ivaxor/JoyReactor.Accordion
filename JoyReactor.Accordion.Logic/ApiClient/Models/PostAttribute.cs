using JoyReactor.Accordion.Logic.ApiClient.Responses;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Models;

public record PostAttribute : NodeResponseObject
{
    /// <summary>
    /// Possible types:
    /// PICTURE,
    /// YOUTUBE, VIMEO, COUB,
    /// SOUNDCLOUD, BANDCAMP
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("image")]
    public Image? Image { get; set; }

    /// <summary>
    /// Possible values:
    /// Youtube/Vimeo/Coub video id,
    /// Soundcloud json { url, height },
    /// BandCamp json { url, height, width }
    /// </summary>
    [JsonPropertyName("value")]
    public string? Value { get; set; }
}
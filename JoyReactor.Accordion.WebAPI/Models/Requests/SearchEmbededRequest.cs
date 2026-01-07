using System.ComponentModel.DataAnnotations;

namespace JoyReactor.Accordion.WebAPI.Models.Requests;

public record SearchEmbededRequest
{
    [Required]
    public SearchEmbeddedType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string Text { get; set; }
}

public enum SearchEmbeddedType
{
    BandCamp,
    Coub,
    SoundCloud,
    Vimeo,
    YouTube,
}
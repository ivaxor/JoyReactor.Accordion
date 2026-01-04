using System.ComponentModel.DataAnnotations;

namespace JoyReactor.Accordion.WebAPI.Models.Requests;

public record SearchTextRequest
{
    [Required]    
    public SearchTextType Type { get; set; }

    [Required]
    [MaxLength(100)]
    public string Text { get; set; }
}

public enum SearchTextType
{
    BandCamp,
    Coub,
    SoundCloud,
    Vimeo,
    YouTube,
}
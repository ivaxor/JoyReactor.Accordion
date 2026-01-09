namespace JoyReactor.Accordion.WebAPI.Models;

public record BackgroundServiceSettings
{
    public TimeSpan SubsequentRunDelay { get; set; }
    public string[] DisabledServiceNames { get; set; }
}
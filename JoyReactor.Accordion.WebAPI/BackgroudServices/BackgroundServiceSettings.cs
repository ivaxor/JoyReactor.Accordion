namespace JoyReactor.Accordion.WebAPI.BackgroudServices;

public record BackgroundServiceSettings
{
    public TimeSpan SubsequentRunDelay { get; set; }
}
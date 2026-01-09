namespace JoyReactor.Accordion.WebAPI.Models;

public record RateLimiterSettings
{
    public int PermitLimit { get; set; }
    public TimeSpan Window { get; set; }
}
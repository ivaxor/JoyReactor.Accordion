using System.Collections.Frozen;
using System.ComponentModel.DataAnnotations;

namespace JoyReactor.Accordion.WebAPI.Models.Requests;

public record SearchDownloadRequest : IValidatableObject
{
    protected static readonly FrozenSet<string> BlacklistedHosts = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "lan",
        "local",
        "internal",
    }.ToFrozenSet();

    [Required]
    [MaxLength(500)]
    public string PictureUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var pictureUri = new Uri(PictureUrl);
        if (!pictureUri.IsAbsoluteUri)
            yield return new ValidationResult("URL must be absolute");

        if (pictureUri.Scheme != Uri.UriSchemeHttps)
            yield return new ValidationResult("Only HTTPS protocol is allowed");

        var host = pictureUri.Host;
        if (Uri.CheckHostName(host) is UriHostNameType.IPv4 or UriHostNameType.IPv6)
            yield return new ValidationResult("IP addresses are not allowed");

        if (BlacklistedHosts.Contains(host))
            yield return new ValidationResult("Local or internal hosts are not allowed");
    }
}

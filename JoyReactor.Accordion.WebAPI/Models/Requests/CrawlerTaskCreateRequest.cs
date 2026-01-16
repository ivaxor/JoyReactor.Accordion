using JoyReactor.Accordion.Logic.ApiClient.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JoyReactor.Accordion.WebAPI.Models.Requests;

public record CrawlerTaskCreateRequest
{
    [Required]
    [MaxLength(100)]
    public string TagName { get; set; }

    [Required]
    [DefaultValue(PostLineType.ALL)]
    public PostLineType PostLineType { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    [DefaultValue(1)]
    public int PageFrom { get; set; }
}
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace JoyReactor.Accordion.WebAPI.Models.Requests;

public record SearchUploadRequest
{
    [Required]
    [FromForm]
    public IFormFile Picture { get; set; }
}
using JoyReactor.Accordion.Logic.Database.Sql.Entities;

namespace JoyReactor.Accordion.WebAPI.Models.Responses;

public record ParsedTagThinResponse
{
    public ParsedTagThinResponse() { }
    public ParsedTagThinResponse(ParsedTag tag)
    {
        Id = tag.Id;
        NumberId = tag.NumberId;
        Name = tag?.Name;
    }

    public Guid Id { get; set; }

    public int NumberId { get; set; }
    public string Name { get; set; }
}
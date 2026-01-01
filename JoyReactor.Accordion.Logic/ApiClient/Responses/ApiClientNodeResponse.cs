using System.Text;
using System.Text.Json.Serialization;

namespace JoyReactor.Accordion.Logic.ApiClient.Responses;

public record ApiClientNodeResponse<T>
    where T : INodeResponseObject
{
    [JsonPropertyName("node")]
    public T Node { get; set; }
}

public interface INodeResponseObject
{
    public string NodeId { get; set; }
    public int NumberId { get; }
}

public record NodeResponseObject : INodeResponseObject
{
    [JsonPropertyName("Id")]
    public string NodeId { get; set; }

    [JsonIgnore]
    public int NumberId => int.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(NodeId)).Split(':').Last());
}
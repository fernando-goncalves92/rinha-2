using System.Text.Json.Serialization;

namespace Api;

public record TransactionPost
{
    [JsonPropertyName("valor")] 
    public object Amount { get; set; }
    
    [JsonPropertyName("tipo")] 
    public char TransactionType { get; set; }
    
    [JsonPropertyName("descricao")] 
    public string Description { get; set; }
}
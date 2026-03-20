using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace Survey.Models;

/// <summary>
/// Cosmos DB ドキュメントモデル（コンテナ: survey, PK: /date）
/// </summary>
public class Feedback
{
    [JsonPropertyName("id")]
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("comment")]
    [JsonProperty("comment")]
    public string Comment { get; set; } = string.Empty;

    /// <summary>
    /// Partition Key — 日付文字列（yyyy-MM-dd, UTC）
    /// </summary>
    [JsonPropertyName("date")]
    [JsonProperty("date")]
    public string Date { get; set; } = DateTime.UtcNow.ToString("yyyy-MM-dd");

    [JsonPropertyName("createdAt")]
    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

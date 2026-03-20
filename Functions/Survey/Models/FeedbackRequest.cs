using System.Text.Json.Serialization;

namespace Survey.Models;

/// <summary>
/// POST /api/feedbacks のリクエストDTO
/// </summary>
public class FeedbackRequest
{
    [JsonPropertyName("comment")]
    public string? Comment { get; set; }
}

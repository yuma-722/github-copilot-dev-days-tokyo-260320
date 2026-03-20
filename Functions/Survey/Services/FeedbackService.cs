using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Survey.Models;

namespace Survey.Services;

public class FeedbackService : IFeedbackService
{
    private readonly Container _container;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(Container container, ILogger<FeedbackService> logger)
    {
        _container = container;
        _logger = logger;
    }

    public async Task<Feedback> CreateAsync(string comment)
    {
        var feedback = new Feedback
        {
            Id = Guid.NewGuid().ToString(),
            Comment = comment,
            Date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            CreatedAt = DateTime.UtcNow
        };

        _logger.LogInformation("Saving feedback {FeedbackId}", feedback.Id);

        var response = await _container.CreateItemAsync(
            feedback,
            new PartitionKey(feedback.Date));

        _logger.LogInformation("Feedback {FeedbackId} saved. RU charge: {RequestCharge}",
            feedback.Id, response.RequestCharge);

        return response.Resource;
    }

    public async Task<List<Feedback>> GetAllAsync()
    {
        var query = new QueryDefinition("SELECT * FROM c ORDER BY c.createdAt DESC");
        var iterator = _container.GetItemQueryIterator<Feedback>(query);

        var results = new List<Feedback>();
        while (iterator.HasMoreResults)
        {
            var page = await iterator.ReadNextAsync();
            results.AddRange(page);
        }

        _logger.LogInformation("Retrieved {Count} feedbacks", results.Count);
        return results;
    }
}

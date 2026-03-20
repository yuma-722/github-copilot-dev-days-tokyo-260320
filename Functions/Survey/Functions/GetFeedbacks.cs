using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Services;

namespace Survey.Functions;

public class GetFeedbacks
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<GetFeedbacks> _logger;

    public GetFeedbacks(IFeedbackService feedbackService, ILogger<GetFeedbacks> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    [Function("GetFeedbacks")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "feedbacks")] HttpRequestData req)
    {
        _logger.LogInformation("GET /api/feedbacks requested");

        try
        {
            var feedbacks = await _feedbackService.GetAllAsync();

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(new FeedbackResponse
            {
                Success = true,
                Feedbacks = feedbacks
            });
            return response;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB エラー: StatusCode={StatusCode}", ex.StatusCode);
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new FeedbackResponse
            {
                Success = false,
                Error = "Internal Server Error"
            });
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラー");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteAsJsonAsync(new FeedbackResponse
            {
                Success = false,
                Error = "Internal Server Error"
            });
            return response;
        }
    }
}

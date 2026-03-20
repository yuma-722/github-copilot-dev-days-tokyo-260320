using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Survey.Models;
using Survey.Services;

namespace Survey.Functions;

public class PostFeedback
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<PostFeedback> _logger;

    public PostFeedback(IFeedbackService feedbackService, ILogger<PostFeedback> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    [Function("PostFeedback")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "feedbacks")] HttpRequestData req)
    {
        _logger.LogInformation("POST /api/feedbacks requested");

        // リクエストボディのデシリアライズ
        FeedbackRequest? body;
        try
        {
            body = await req.ReadFromJsonAsync<FeedbackRequest>();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "リクエストボディの解析に失敗");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "リクエストボディが不正です");
        }

        if (body is null)
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "リクエストボディが不正です");
        }

        // バリデーション
        if (string.IsNullOrWhiteSpace(body.Comment))
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "comment は必須です");
        }

        if (body.Comment.Length > 2000)
        {
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "comment は2000文字以内で入力してください");
        }

        // 保存
        try
        {
            var feedback = await _feedbackService.CreateAsync(body.Comment);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteAsJsonAsync(new FeedbackResponse
            {
                Success = true,
                Id = feedback.Id,
                CreatedAt = feedback.CreatedAt
            });
            return response;
        }
        catch (CosmosException ex)
        {
            _logger.LogError(ex, "Cosmos DB エラー: StatusCode={StatusCode}, SubStatusCode={SubStatusCode}",
                ex.StatusCode, ex.SubStatusCode);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal Server Error");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "予期しないエラー");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "Internal Server Error");
        }
    }

    private static async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req, HttpStatusCode statusCode, string error)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new FeedbackResponse
        {
            Success = false,
            Error = error
        });
        return response;
    }
}

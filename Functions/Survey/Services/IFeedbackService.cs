using Survey.Models;

namespace Survey.Services;

public interface IFeedbackService
{
    /// <summary>
    /// 感想を1件 Cosmos DB に保存する
    /// </summary>
    Task<Feedback> CreateAsync(string comment);

    /// <summary>
    /// 全件取得（createdAt 降順）
    /// </summary>
    Task<List<Feedback>> GetAllAsync();
}

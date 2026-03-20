using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Survey.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Cosmos DB — CosmosClient を DI 登録し、Container を直接解決可能にする
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var connectionString = config["CosmosDb:ConnectionString"];
    var accountEndpoint = config["CosmosDb:AccountEndpoint"];

    // ローカル: 接続文字列、Azure: Managed Identity（AccountEndpoint のみ）
    if (!string.IsNullOrEmpty(connectionString))
    {
        return new CosmosClient(connectionString);
    }

    return new CosmosClient(accountEndpoint, new Azure.Identity.DefaultAzureCredential());
});

builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var client = sp.GetRequiredService<CosmosClient>();
    var databaseName = config["CosmosDb:DatabaseName"] ?? "ghdevdays";
    var containerName = config["CosmosDb:ContainerName"] ?? "survey";
    return client.GetContainer(databaseName, containerName);
});

builder.Services.AddSingleton<IFeedbackService, FeedbackService>();

builder.Build().Run();

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class GamesIndexUpdater
{
    private readonly SearchClient _searchClient;
    private readonly ILogger _logger;
    public GamesIndexUpdater(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<GamesIndexUpdater>();
        var endpoint = Environment.GetEnvironmentVariable("Search__Endpoint") ?? "";
        var apiKey = Environment.GetEnvironmentVariable("Search__ApiKey") ?? "";
        var index = Environment.GetEnvironmentVariable("Search__IndexName") ?? "games";
        if (!string.IsNullOrEmpty(endpoint) && !string.IsNullOrEmpty(apiKey))
        {
            _searchClient = new SearchClient(new Uri(endpoint), index, new Azure.AzureKeyCredential(apiKey));
        }
    }

    [Function("GamesIndexUpdater")]
    public async Task Run([QueueTrigger("games-events", Connection = "AzureWebJobsStorage")] string message)
    {
        try
        {
            var evt = JsonSerializer.Deserialize<Dictionary<string, object>>(message);
            if (_searchClient == null || evt == null) return;
            if (evt.TryGetValue("Type", out var type) && type?.ToString() is string t && (t == "GameCreated" || t == "GameUpdated"))
            {
                await _searchClient.MergeOrUploadDocumentsAsync(new[] { evt });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update index");
            throw;
        }
    }
}

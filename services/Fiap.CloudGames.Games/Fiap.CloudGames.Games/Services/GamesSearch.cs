using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

public interface IGamesSearch
{
    Task<IEnumerable<object>> SearchAsync(string query);
}

public class AzureSearchGamesSearch : IGamesSearch
{
    private readonly SearchClient _client;
    public AzureSearchGamesSearch(IConfiguration configuration)
    {
        var endpoint = new Uri(configuration["Search:Endpoint"]!);
        var apiKey = new AzureKeyCredential(configuration["Search:ApiKey"]!);
        var indexName = configuration["Search:IndexName"] ?? "games";
        _client = new SearchClient(endpoint, indexName, apiKey);
    }

    public async Task<IEnumerable<object>> SearchAsync(string query)
    {
        var resp = await _client.SearchAsync<SearchDocument>(string.IsNullOrWhiteSpace(query) ? "*" : query);
        var results = new List<object>();
        await foreach (var r in resp.Value.GetResultsAsync())
        {
            results.Add(r.Document);
        }
        return results;
    }
}

public class NoopGamesSearch : IGamesSearch
{
    private readonly GamesDbContext _db;
    public NoopGamesSearch(GamesDbContext db) { _db = db; }

    public async Task<IEnumerable<object>> SearchAsync(string query)
    {
        var q = (query ?? string.Empty).Trim();
        var baseQuery = _db.Games.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
        {
            baseQuery = baseQuery.Where(g => g.Title.Contains(q) || g.Description.Contains(q));
        }
        var items = await baseQuery
            .Select(g => new { g.Id, g.Title, g.Price, g.Genre })
            .ToListAsync();
        return items;
    }
}

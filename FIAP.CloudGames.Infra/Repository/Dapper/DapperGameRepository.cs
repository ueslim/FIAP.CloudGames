using Dapper;
using FIAP.CloudGames.Domain.Entities;
using FIAP.CloudGames.Domain.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace FIAP.CloudGames.Infra.Repository.Dapper
{
    public class DapperGameRepository : IDapperGameRepository
    {
        private readonly string _connectionString;

        public DapperGameRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new ArgumentNullException("SqlServer connection string not configured");
        }

        private async Task<SqlConnection> CreateOpenConnectionAsync()
        {
            var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        public async Task<IEnumerable<Game>> GetAllGamesWithDapperAsync()
        {
            await using var connection = await CreateOpenConnectionAsync();

            // Query that returns all Game properties plus the raw Tags string
            var results = await connection.QueryAsync<Game, string, Game>(
                sql: @"
            SELECT
                g.Id,
                g.Title,
                g.Description,
                g.Developer,
                g.Publisher,
                g.Genre,
                g.ReleaseDate,
                g.Price,
                g.CoverImageUrl,
                g.IsActive,
                g.Tags
            FROM
                Games g
            WHERE
                g.IsActive = 1
            ORDER BY
                g.Title",
                map: (game, tags) =>
                {
                    game.Tags = ConvertTagsStringToArray(tags);
                    return game;
                },
                splitOn: "Tags"  // The column name where the split should occur
            );

            return results;
        }

        public async Task<IEnumerable<Game>> GetUserLibraryWithDapperAsync(Guid userId)
        {
            await using var connection = await CreateOpenConnectionAsync();

            // Query that returns Game properties, UserGame properties, and raw Tags
            var results = await connection.QueryAsync<Game, UserGame, string, Game>(
                sql: @"
            SELECT
                g.Id,
                g.Title,
                g.Description,
                g.Developer,
                g.Publisher,
                g.Genre,
                g.ReleaseDate,
                g.Price,
                g.CoverImageUrl,
                g.IsActive,
                ug.Id,
                ug.UserId,
                ug.GameId,
                ug.PurchaseDate,
                ug.PurchasePrice,
                ug.IsActive,
                g.Tags
            FROM
                UserGames ug
            INNER JOIN
                Games g ON ug.GameId = g.Id
            WHERE
                ug.UserId = @UserId
                AND ug.IsActive = 1
                AND g.IsActive = 1
            ORDER BY
                g.Title",
                map: (game, userGame, tags) =>
                {
                    game.Tags = ConvertTagsStringToArray(tags);
                    game.UserGames = new List<UserGame> { userGame };
                    return game;
                },
                param: new { UserId = userId },
                splitOn: "Id,Id,Tags"  // Split points for each object type
            );

            return results;
        }

        private string[] ConvertTagsStringToArray(string tagsString)
        {
            if (string.IsNullOrWhiteSpace(tagsString))
                return Array.Empty<string>();

            return tagsString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(tag => tag.Trim())
                            .ToArray();
        }
    }
}
namespace FIAP.CloudGames.Application.DTOs
{
    public class GameDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }
        public string Genre { get; set; }
        public string[] Tags { get; set; }
    }

    public class CreateGameDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string Developer { get; set; }
        public required string Publisher { get; set; }
        public required DateTime ReleaseDate { get; set; }
        public required decimal Price { get; set; }
        public required string CoverImageUrl { get; set; }
        public required string[] Tags { get; set; }
        public required string Genre { get; set; }
    }

    public class UpdateGameDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public decimal? Price { get; set; }
        public string? CoverImageUrl { get; set; }
        public string[]? Tags { get; set; }
        public string? Genre { get; set; }
    }

    public class PurchaseGameDto
    {
        public Guid GameId { get; set; }
    }
}
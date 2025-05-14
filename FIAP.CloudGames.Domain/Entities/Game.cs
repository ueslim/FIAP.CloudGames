namespace FIAP.CloudGames.Domain.Entities
{
    public class Game : BaseEntity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }
        public decimal Price { get; set; }
        public string CoverImageUrl { get; set; }
        public string[] Tags { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<UserGame> UserGames { get; set; } = new List<UserGame>();
    }
}
namespace FIAP.CloudGames.Domain.Entities
{
    public class UserGame
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid GameId { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchasePrice { get; set; }
        public bool IsActive { get; set; }
        public virtual User User { get; set; }
        public virtual Game Game { get; set; }
    }
}
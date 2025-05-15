namespace FIAP.CloudGames.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public UserRole Role { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; }
        public virtual ICollection<UserGame> UserGames { get; set; } = [];
    }

    public enum UserRole
    {
        User = 0,
        Administrator = 1
    }
}
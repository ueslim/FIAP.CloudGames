using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infra.Configurations
{
    public class UserGameConfiguration : IEntityTypeConfiguration<UserGame>
    {
        public void Configure(EntityTypeBuilder<UserGame> builder)
        {
            builder.ToTable("UserGames");

            builder.HasKey(ug => ug.Id);

            builder.Property(ug => ug.PurchaseDate)
                .IsRequired();

            builder.Property(ug => ug.PurchasePrice)
                .HasColumnType("decimal(18,2)");

            builder.Property(ug => ug.IsActive)
                .HasDefaultValue(true);

            // Relationships
            builder.HasOne(ug => ug.User)
                .WithMany(u => u.UserGames)
                .HasForeignKey(ug => ug.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(ug => ug.Game)
                .WithMany(g => g.UserGames)
                .HasForeignKey(ug => ug.GameId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
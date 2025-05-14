using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infra.Configurations
{
    public class GameConfiguration : IEntityTypeConfiguration<Game>
    {
        public void Configure(EntityTypeBuilder<Game> builder)
        {
            builder.ToTable("Games");

            builder.HasKey(g => g.Id);

            builder.Property(g => g.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(g => g.Developer)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Publisher)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Price)
                .HasColumnType("decimal(18,2)");

            builder.Property(g => g.CoverImageUrl)
                .HasMaxLength(500);

            builder.Property(g => g.IsActive)
                .HasDefaultValue(true);

            builder.Property(g => g.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            // Relationships
            builder.HasMany(g => g.UserGames)
                .WithOne(ug => ug.Game)
                .HasForeignKey(ug => ug.GameId);
        }
    }
}
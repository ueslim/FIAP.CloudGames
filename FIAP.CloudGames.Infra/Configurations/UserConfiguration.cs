using FIAP.CloudGames.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Infra.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                 .HasColumnName("Id")
                 .IsRequired()
                 .ValueGeneratedOnAdd()
                 .HasDefaultValueSql("NEWID()");

            builder.Property(u => u.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.PasswordHash)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(u => u.Role)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(u => u.IsActive)
                .HasDefaultValue(true);

            // Relationships
            builder.HasMany(u => u.UserGames)
                .WithOne(ug => ug.User)
                .HasForeignKey(ug => ug.UserId);
        }
    }
}
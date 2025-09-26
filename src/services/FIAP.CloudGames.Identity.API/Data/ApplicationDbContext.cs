using FIAP.CloudGames.Identity.API.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NetDevPack.Security.Jwt.Core.Model;
using NetDevPack.Security.Jwt.Store.EntityFrameworkCore;

namespace FIAP.CloudGames.Identity.API.Data
{
    public class ApplicationDbContext : IdentityDbContext, ISecurityKeyContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<KeyMaterial> SecurityKeys { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("identity");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RefreshToken>(e =>
            {
                e.ToTable("RefreshTokens");
                e.HasKey(x => x.Id);
                e.Property(x => x.Username).HasMaxLength(256).IsRequired();
                e.Property(x => x.Token).HasMaxLength(512).IsRequired();
                e.Property(x => x.ExpirationDate).IsRequired();
                e.HasIndex(x => x.Username);
            });
        }
    }
}
using FIAP.CloudGames.Cart.API.Model;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace FIAP.CloudGames.Cart.API.Data
{
    public sealed class CartContext : DbContext
    {
        public CartContext(DbContextOptions<CartContext> options)
            : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<CartCustomer> CartCustomer { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("cart");

            foreach (var prop in modelBuilder.Model.GetEntityTypes()
                         .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
            {
                prop.SetColumnType("varchar(100)");
            }

            modelBuilder.Ignore<ValidationResult>();

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(CartContext).Assembly);

            foreach (var fk in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
                fk.DeleteBehavior = DeleteBehavior.Cascade;

            base.OnModelCreating(modelBuilder);
        }
    }
}
using FIAP.CloudGames.Cart.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Cart.API.Data.Mappings
{
    public class CartItemMapping : IEntityTypeConfiguration<CartItem>
    {
        public void Configure(EntityTypeBuilder<CartItem> builder)
        {
            builder.HasKey(i => i.Id);

            builder.Property(i => i.Name)
                   .IsRequired()
                   .HasColumnType("varchar(100)");

            builder.Property(i => i.Image)
                   .IsRequired()
                   .HasColumnType("varchar(100)");

            builder.Property(i => i.Value)
                   .HasColumnType("decimal(18,2)");

            builder.ToTable("CartItems");
        }
    }
}
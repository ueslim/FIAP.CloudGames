using FIAP.CloudGames.Cart.API.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Cart.API.Data.Mappings
{
    public class CartCustomerMapping : IEntityTypeConfiguration<CartCustomer>
    {
        public void Configure(EntityTypeBuilder<CartCustomer> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.TotalValue)
                   .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Discount)
                   .HasColumnType("decimal(18,2)");

            builder.HasIndex(c => c.CustomerId)
                   .HasDatabaseName("IDX_Customer");

            // 1 : N  CartCustomer -> CartItems
            builder.HasMany(c => c.Items)
                   .WithOne(i => i.CartCustomer)
                   .HasForeignKey(i => i.CartId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Owned: Voucher (opcional) com nomes de coluna explícitos
            builder.OwnsOne(c => c.Voucher, v =>
            {
                v.Property(p => p.Code)
                    .HasColumnName("VoucherCode")
                    .HasColumnType("varchar(50)")
                    .IsRequired(false);

                v.Property(p => p.Percentage)
                    .HasColumnName("VoucherPercentage")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                v.Property(p => p.DiscountValue)
                    .HasColumnName("VoucherDiscountValue")
                    .HasColumnType("decimal(18,2)")
                    .IsRequired(false);

                v.Property(p => p.VoucherDiscountType)
                    .HasColumnName("VoucherDiscountType")
                    .IsRequired(false);
            });

            // torna a navegação owned opcional
            builder.Navigation(c => c.Voucher).IsRequired(false);

            builder.ToTable("CartCustomer");
        }
    }
}
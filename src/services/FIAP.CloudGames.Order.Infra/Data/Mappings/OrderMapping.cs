using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Order.Infra.Data.Mappings
{
    public class OrderMapping : IEntityTypeConfiguration<Domain.Order.Order>
    {
        public void Configure(EntityTypeBuilder<Domain.Order.Order> builder)
        {
            builder.HasKey(c => c.Id);

            builder.OwnsOne(p => p.Address, e =>
            {
                e.Property(pe => pe.Street)
       .HasColumnName("Street");

                e.Property(pe => pe.Number)
                    .HasColumnName("Number");

                e.Property(pe => pe.AdditionalInfo)
                    .HasColumnName("AdditionalInfo");

                e.Property(pe => pe.Neighborhood)
                    .HasColumnName("Neighborhood");

                e.Property(pe => pe.PostalCode)
                    .HasColumnName("PostalCode");

                e.Property(pe => pe.City)
                    .HasColumnName("City");

                e.Property(pe => pe.State)
                    .HasColumnName("State");
            });

            builder.Property(c => c.Code)
                .HasDefaultValueSql("NEXT VALUE FOR [order].[MySequence]");

            // 1 : N => Pedido : PedidoItems
            builder.HasMany(c => c.OrderItems)
                .WithOne(c => c.Order)
                .HasForeignKey(c => c.OrderId);

            builder.ToTable("Orders");
        }
    }
}
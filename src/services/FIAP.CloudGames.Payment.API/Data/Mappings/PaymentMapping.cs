using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FIAP.CloudGames.Payment.API.Data.Mappings
{
    public class PaymentMapping : IEntityTypeConfiguration<Models.Payment>
    {
        public void Configure(EntityTypeBuilder<Models.Payment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Ignore(c => c.CreditCard);

            // 1 : N => Pagamento : Transacao
            builder.HasMany(c => c.Transactions)
                .WithOne(c => c.Payment)
                .HasForeignKey(c => c.PaymentId);

            builder.ToTable("Payments");
        }
    }
}
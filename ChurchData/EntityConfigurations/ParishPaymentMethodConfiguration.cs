using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class ParishPaymentMethodConfiguration : IEntityTypeConfiguration<ParishPaymentMethod>
    {
        public void Configure(EntityTypeBuilder<ParishPaymentMethod> builder)
        {
            builder.ToTable("parish_payment_methods");
            builder.HasKey(p => p.PaymentMethodId);
            builder.Property(p => p.PaymentMethodId).HasColumnName("payment_method_id");
            builder.Property(p => p.ParishId).HasColumnName("parish_id").IsRequired();
            builder.Property(p => p.MethodType).HasColumnName("method_type").HasMaxLength(50).IsRequired();
            builder.Property(p => p.UpiId).HasColumnName("upi_id").HasMaxLength(255);
            builder.Property(p => p.BankId).HasColumnName("bank_id");
            builder.Property(p => p.DisplayName).HasColumnName("display_name").HasMaxLength(255).IsRequired();
            builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
            builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();

            builder.HasOne(p => p.Parish)
                      .WithMany(par => par.PaymentMethods)
                      .HasForeignKey(p => p.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(p => p.Bank)
                      .WithMany()
                      .HasForeignKey(p => p.BankId)
                      .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

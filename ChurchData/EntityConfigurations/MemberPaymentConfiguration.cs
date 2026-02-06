using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class MemberPaymentConfiguration : IEntityTypeConfiguration<MemberPayment>
    {
        public void Configure(EntityTypeBuilder<MemberPayment> builder)
        {
            builder.ToTable("member_payments");
            builder.HasKey(m => m.PaymentId);

            builder.Property(m => m.PaymentId).HasColumnName("payment_id").HasColumnType("uuid");
            builder.Property(m => m.ReceiptId).HasColumnName("receipt_id").HasMaxLength(6).IsRequired();
            builder.Property(m => m.ParishId).HasColumnName("parish_id").IsRequired();
            builder.Property(m => m.FamilyId).HasColumnName("family_id").IsRequired();
            builder.Property(m => m.MemberId).HasColumnName("member_id");
            builder.Property(m => m.HeadId).HasColumnName("head_id").IsRequired();
            builder.Property(m => m.PaymentMethodId).HasColumnName("payment_method_id").IsRequired();
            builder.Property(m => m.BankId).HasColumnName("bank_id");
            builder.Property(m => m.Amount).HasColumnName("amount").HasColumnType("numeric(15,2)").IsRequired();
            builder.Property(m => m.PaymentMode).HasColumnName("payment_mode").HasMaxLength(20).IsRequired();
            builder.Property(m => m.UtrNumber).HasColumnName("utr_number").HasMaxLength(50);
            builder.Property(m => m.ReferenceNumber).HasColumnName("reference_number").HasMaxLength(50);
            builder.Property(m => m.PaymentDate).HasColumnName("payment_date").IsRequired();
            builder.Property(m => m.ReceivedAt).HasColumnName("received_at").IsRequired();
            builder.Property(m => m.Status).HasColumnName("status").HasMaxLength(20).IsRequired().HasDefaultValue("PENDING");
            builder.Property(m => m.ApprovedAt).HasColumnName("approved_at");
            builder.Property(m => m.ApprovedBy).HasColumnName("approved_by");
            builder.Property(m => m.Remarks).HasColumnName("remarks");
            builder.Property(m => m.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(m => m.CreatedBy).HasColumnName("created_by").IsRequired();

            builder.HasOne(m => m.Parish)
                      .WithMany()
                      .HasForeignKey(m => m.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Family)
                      .WithMany()
                      .HasForeignKey(m => m.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(m => m.Member)
                      .WithMany()
                      .HasForeignKey(m => m.MemberId)
                      .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(m => m.Head)
                      .WithMany()
                      .HasForeignKey(m => m.HeadId)
                      .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.PaymentMethod)
                      .WithMany()
                      .HasForeignKey(m => m.PaymentMethodId)
                      .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Bank)
                      .WithMany()
                      .HasForeignKey(m => m.BankId)
                      .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(m => m.CreatedByUser)
                      .WithMany()
                      .HasForeignKey(m => m.CreatedBy)
                      .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(m => m.ApprovedBy)
                      .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

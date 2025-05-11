using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.Configurations
{
    public class RecurringTransactionConfiguration : IEntityTypeConfiguration<RecurringTransaction>
    {
        public void Configure(EntityTypeBuilder<RecurringTransaction> builder)
        {
            builder.ToTable("recurring_transactions");

            builder.HasKey(e => e.RepeatedEntryId);

            builder.Property(e => e.RepeatedEntryId).HasColumnName("recurring_id");
            builder.Property(e => e.HeadId).HasColumnName("head_id");
            builder.Property(e => e.FamilyId).HasColumnName("family_id");
            builder.Property(e => e.ParishId).HasColumnName("parish_id");
          builder.Property(e => e.BillName)
                   .IsRequired()
                   .HasMaxLength(100)
                   .HasColumnName("billname");

            builder.Property(e => e.IncomeAmount).HasColumnName("income_amount");

            // Foreign Key Constraints
            builder.HasOne<TransactionHead>()
                   .WithMany()
                   .HasForeignKey(e => e.HeadId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Family>()
                   .WithMany()
                   .HasForeignKey(e => e.FamilyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Parish>()
                   .WithMany()
                   .HasForeignKey(e => e.ParishId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace YourNamespace.Data.Configurations
{
    public class FamilyContributionConfiguration : IEntityTypeConfiguration<FamilyContribution>
    {
        public void Configure(EntityTypeBuilder<FamilyContribution> builder)
        {
            builder.ToTable("family_contributions");

            builder.HasKey(fc => fc.ContributionId);

            builder.Property(fc => fc.ContributionId).HasColumnName("contribution_id");
            builder.Property(fc => fc.TransactionDate).HasColumnName("tr_date").IsRequired();
            builder.Property(fc => fc.VoucherNumber).HasColumnName("vr_no").IsRequired().HasMaxLength(50);
            builder.Property(fc => fc.TransactionType).HasColumnName("transaction_type").HasMaxLength(10);
            builder.Property(fc => fc.HeadId).HasColumnName("head_id").IsRequired();
            builder.Property(fc => fc.FamilyId).HasColumnName("family_id").IsRequired();
            builder.Property(fc => fc.BankId).HasColumnName("bank_id").IsRequired();
            builder.Property(fc => fc.ParishId).HasColumnName("parish_id").IsRequired();
            builder.Property(fc => fc.SettingId).HasColumnName("setting_id").IsRequired();
            builder.Property(fc => fc.IncomeAmount).HasColumnName("income_amount").HasDefaultValue(0);
            builder.Property(fc => fc.ExpenseAmount).HasColumnName("expense_amount").HasDefaultValue(0);
            builder.Property(fc => fc.Description).HasColumnName("description");
            builder.Property(fc => fc.BillName).HasColumnName("bill_name");

            // New Columns Configuration
            builder.Property(t => t.CreatedAt)
                   .HasColumnName("created_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired();

            builder.Property(t => t.CreatedBy)
                   .HasColumnName("created_by")
                   .IsRequired();

            builder.Property(t => t.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasColumnType("timestamp with time zone")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP")
                   .IsRequired(false);

            builder.Property(t => t.UpdatedBy)
                   .HasColumnName("updated_by")
                   .IsRequired(false);

            builder.Property(t => t.BillName)
                   .HasColumnName("bill_name")
                   .HasMaxLength(255);


            // Constraints (Check Constraints)
            builder.HasCheckConstraint("contribution_expense_amount_check", "expense_amount >= 0");
            builder.HasCheckConstraint("contribution_income_amount_check", "income_amount >= 0");
            builder.HasCheckConstraint("transaction_type_check", "transaction_type IN ('Income', 'Expense')");

            // Foreign Key Constraints
            builder.HasOne<TransactionHead>()
                   .WithMany()
                   .HasForeignKey(fc => fc.HeadId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Family>()
                   .WithMany()
                   .HasForeignKey(fc => fc.FamilyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Bank>()
                   .WithMany()
                   .HasForeignKey(fc => fc.BankId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Parish>()
                   .WithMany()
                   .HasForeignKey(fc => fc.ParishId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>() // Assuming User entity is available
              .WithMany()
              .HasForeignKey(t => t.CreatedBy)
              .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>() // Assuming User entity is available
                   .WithMany()
                   .HasForeignKey(t => t.UpdatedBy)
                   .OnDelete(DeleteBehavior.NoAction);

            // Uncomment this if ContributionSettings mapping is required
            // builder.HasOne<ContributionSettings>()
            //        .WithMany()
            //        .HasForeignKey(fc => fc.SettingId)
            //        .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

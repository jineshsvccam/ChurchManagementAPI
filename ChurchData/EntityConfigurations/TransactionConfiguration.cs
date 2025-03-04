using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.ToTable("transactions", t =>
            {
                t.HasCheckConstraint("transaction_expense_amount_check", "expense_amount >= 0");
                t.HasCheckConstraint("transaction_income_amount_check", "income_amount >= 0");
                t.HasCheckConstraint("transaction_type_check", "transaction_type IN ('Income', 'Expense')");
            });

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.TransactionId)
                   .HasColumnName("transaction_id");

            builder.Property(t => t.TrDate)
                   .HasColumnName("tr_date")
                   .IsRequired()
                   .HasColumnType("date");

            builder.Property(t => t.VrNo)
                   .HasColumnName("vr_no")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.TransactionType)
                   .HasColumnName("transaction_type")
                   .HasMaxLength(10);

            builder.Property(t => t.HeadId)
                   .HasColumnName("head_id");

            builder.Property(t => t.FamilyId)
                   .HasColumnName("family_id");

            builder.Property(t => t.BankId)
                   .HasColumnName("bank_id");

            builder.Property(t => t.IncomeAmount)
                   .HasColumnName("income_amount")
                   .HasColumnType("numeric(15,2)")
                   .HasDefaultValue(0);

            builder.Property(t => t.ExpenseAmount)
                   .HasColumnName("expense_amount")
                   .HasColumnType("numeric(15,2)")
                   .HasDefaultValue(0);

            builder.Property(t => t.Description)
                   .HasColumnName("description")
                   .HasColumnType("text");

            builder.Property(t => t.ParishId)
                   .HasColumnName("parish_id")
                   .IsRequired();

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

            // Foreign Key Constraints
            builder.HasOne(t => t.TransactionHead)
                   .WithMany(th => th.Transactions)
                   .HasForeignKey(t => t.HeadId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.Family)
                   .WithMany(f => f.Transactions)
                   .HasForeignKey(t => t.FamilyId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.Bank)
                   .WithMany(b => b.Transactions)
                   .HasForeignKey(t => t.BankId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(t => t.Parish)
                   .WithMany(p => p.Transactions)
                   .HasForeignKey(t => t.ParishId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>() // Assuming User entity is available
                   .WithMany()
                   .HasForeignKey(t => t.CreatedBy)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne<User>() // Assuming User entity is available
                   .WithMany()
                   .HasForeignKey(t => t.UpdatedBy)
                   .OnDelete(DeleteBehavior.NoAction);
        }
    }
}

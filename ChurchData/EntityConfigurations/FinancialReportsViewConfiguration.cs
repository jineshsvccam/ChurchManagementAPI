using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class FinancialReportsViewConfiguration : IEntityTypeConfiguration<FinancialReportsView>
    {
        public void Configure(EntityTypeBuilder<FinancialReportsView> builder)
        {
            builder.ToView("financialreportsview");
            builder.HasNoKey();

            builder.Property(e => e.TransactionId).HasColumnName("transactionid");
            builder.Property(e => e.TrDate).HasColumnName("trdate");
            builder.Property(e => e.VrNo).HasColumnName("vrno");
            builder.Property(e => e.TransactionType).HasColumnName("transactiontype");
            builder.Property(e => e.IncomeAmount).HasColumnName("incomeamount");
            builder.Property(e => e.ExpenseAmount).HasColumnName("expenseamount");
            builder.Property(e => e.Description).HasColumnName("description");
            builder.Property(e => e.ParishId).HasColumnName("parishid");
            builder.Property(e => e.ParishName).HasColumnName("parishname");
            builder.Property(e => e.FamilyId).HasColumnName("familyid");
            builder.Property(e => e.FamilyName).HasColumnName("familyname");
            builder.Property(e => e.HeadId).HasColumnName("headid");
            builder.Property(e => e.HeadName).HasColumnName("headname");
            builder.Property(e => e.BankId).HasColumnName("bankid");
            builder.Property(e => e.BankName).HasColumnName("bankname");
            builder.Property(e => e.UnitId).HasColumnName("unitid");
            builder.Property(e => e.UnitName).HasColumnName("unitname");
            builder.Property(e => e.FamilyNumber).HasColumnName("familynumber");
        }
    }
}

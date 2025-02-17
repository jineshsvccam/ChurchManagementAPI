using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class FinancialYearConfiguration : IEntityTypeConfiguration<FinancialYear>
    {
        public void Configure(EntityTypeBuilder<FinancialYear> builder)
        {
            builder.ToTable("financial_years");

            builder.HasKey(fy => fy.FinancialYearId);

            builder.Property(fy => fy.FinancialYearId)
                   .HasColumnName("financial_year_id");

            builder.Property(fy => fy.ParishId)
                   .HasColumnName("parish_id")
                   .IsRequired();

            builder.Property(fy => fy.StartDate)
                   .HasColumnName("start_date")
                   .IsRequired();

            builder.Property(fy => fy.EndDate)
                   .HasColumnName("end_date")
                   .IsRequired();

            builder.Property(fy => fy.IsLocked)
                   .HasColumnName("is_locked")
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(fy => fy.LockDate)
                   .HasColumnName("lock_date");

            builder.Property(fy => fy.Description)
                   .HasColumnName("description");

            builder.HasOne(fy => fy.Parish)
                   .WithMany(p => p.FinancialYears)
                   .HasForeignKey(fy => fy.ParishId);
        }
    }
}

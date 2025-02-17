using System.Reflection.Emit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class BankConfiguration : IEntityTypeConfiguration<Bank>
    {
        public void Configure(EntityTypeBuilder<Bank> builder)
        {

            builder.ToTable("banks");
            builder.HasKey(b => b.BankId);
            builder.Property(b => b.BankId).HasColumnName("bank_id");
            builder.Property(b => b.BankName).HasColumnName("bank_name").IsRequired().HasMaxLength(100);
            builder.Property(b => b.AccountNumber).HasColumnName("account_number").HasMaxLength(50);
            builder.Property(b => b.OpeningBalance).HasColumnName("opening_balance").HasColumnType("numeric(15,2)").IsRequired();
            builder.Property(b => b.CurrentBalance).HasColumnName("current_balance").HasColumnType("numeric(15,2)").IsRequired();
            builder.Property(b => b.ParishId).HasColumnName("parish_id");

            builder.HasOne(b => b.Parish)
                      .WithMany(p => p.Banks)
                      .HasForeignKey(b => b.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);


        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class TransactionHeadConfiguration : IEntityTypeConfiguration<TransactionHead>
    {
        public void Configure(EntityTypeBuilder<TransactionHead> builder)
        {
            builder.ToTable("transaction_heads", t =>
            {
                t.HasCheckConstraint("transaction_head_type_check", "type IN ('Income', 'Expense', 'Both')");
            });

            builder.HasKey(t => t.HeadId);

            builder.Property(t => t.HeadId).HasColumnName("head_id");
            builder.Property(t => t.HeadName)
                   .HasColumnName("head_name")
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(t => t.Type)
                   .HasColumnName("type")
                   .HasMaxLength(10);
            builder.Property(t => t.IsMandatory)
                   .HasColumnName("is_mandatory")
                   .HasDefaultValue(false);
            builder.Property(t => t.Description).HasColumnName("description");
            builder.Property(t => t.ParishId)
                   .HasColumnName("parish_id")
                   .IsRequired();
            builder.Property(t => t.Aramanapct)
                   .HasColumnName("aramanapct")
                   .HasColumnType("double precision");
            builder.Property(t => t.Ordr)
                   .HasColumnName("ordr")
                   .HasMaxLength(10);
            builder.Property(t => t.HeadNameMl)
                   .HasColumnName("head_name_ml")
                   .HasMaxLength(100);

            builder.HasOne(t => t.Parish)
                   .WithMany(p => p.TransactionHeads)
                   .HasForeignKey(t => t.ParishId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

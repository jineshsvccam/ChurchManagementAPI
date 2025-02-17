using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class FamilyConfiguration : IEntityTypeConfiguration<Family>
    {
        public void Configure(EntityTypeBuilder<Family> builder)
        {
            builder.ToTable("families");

            builder.Property(f => f.FamilyId).HasColumnName("family_id");
            builder.Property(f => f.UnitId).HasColumnName("unit_id");
            builder.Property(f => f.FamilyName)
                   .HasColumnName("family_name")
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(f => f.Address).HasColumnName("address");
            builder.Property(f => f.ContactInfo)
                   .HasColumnName("contact_info")
                   .HasMaxLength(100);
            builder.Property(f => f.Category)
                   .HasColumnName("category")
                   .HasMaxLength(10);
            builder.Property(f => f.FamilyNumber).HasColumnName("family_number");
            builder.Property(f => f.Status)
                   .HasColumnName("status")
                   .HasMaxLength(10);
            builder.Property(f => f.HeadName)
                   .HasColumnName("head_name")
                   .IsRequired()
                   .HasMaxLength(50);
            builder.Property(f => f.ParishId)
                   .HasColumnName("parish_id")
                   .IsRequired();
            builder.Property(f => f.JoinDate)
                   .HasColumnName("join_date")
                   .HasColumnType("date");

            builder.HasOne(f => f.Unit)
                   .WithMany(u => u.Families) // Assuming Unit has a collection of Families
                   .HasForeignKey(f => f.UnitId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

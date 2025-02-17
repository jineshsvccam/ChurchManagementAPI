using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class DioceseConfiguration : IEntityTypeConfiguration<Diocese>
    {
        public void Configure(EntityTypeBuilder<Diocese> builder)
        {
            builder.ToTable("dioceses");
            builder.HasKey(d => d.DioceseId);

            builder.Property(d => d.DioceseId).HasColumnName("diocese_id");
            builder.Property(d => d.DioceseName).HasColumnName("diocese_name");
            builder.Property(d => d.Address).HasColumnName("address");
            builder.Property(d => d.ContactInfo).HasColumnName("contact_info");
            builder.Property(d => d.Territory).HasColumnName("territory");

            builder.HasMany(d => d.Districts)
                   .WithOne(d => d.Diocese)
                   .HasForeignKey(d => d.DioceseId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

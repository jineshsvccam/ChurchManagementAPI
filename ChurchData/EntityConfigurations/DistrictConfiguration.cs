using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class DistrictConfiguration : IEntityTypeConfiguration<District>
    {
        public void Configure(EntityTypeBuilder<District> builder)
        {
            builder.ToTable("ecclesiastical_districts");
            builder.HasKey(d => d.DistrictId);

            builder.Property(d => d.DistrictId).HasColumnName("district_id");
            builder.Property(d => d.DistrictName).HasColumnName("district_name");
            builder.Property(d => d.DioceseId).HasColumnName("diocese_id");
            builder.Property(d => d.Description).HasColumnName("description");

            builder.HasOne(d => d.Diocese)
                   .WithMany(d => d.Districts)
                   .HasForeignKey(d => d.DioceseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(d => d.Parishes)
                   .WithOne(p => p.District)
                   .HasForeignKey(p => p.DistrictId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ChurchData.EntityConfigurations
{
    public class ParishConfiguration : IEntityTypeConfiguration<Parish>
    {
        public void Configure(EntityTypeBuilder<Parish> builder)
        {
            builder.ToTable("parishes");

            builder.Property(p => p.ParishId).HasColumnName("parish_id");
            builder.Property(p => p.ParishName).HasColumnName("parish_name");
            builder.Property(p => p.ParishLocation).HasColumnName("parish_location");
            builder.Property(p => p.Photo).HasColumnName("photo");
            builder.Property(p => p.Address).HasColumnName("address");
            builder.Property(p => p.Phone).HasColumnName("phone");
            builder.Property(p => p.Email).HasColumnName("email");
            builder.Property(p => p.Place).HasColumnName("place");
            builder.Property(p => p.Pincode).HasColumnName("pincode");
            builder.Property(p => p.VicarName).HasColumnName("vicar_name");
            builder.Property(p => p.DistrictId).HasColumnName("district_id");

            builder.HasOne(p => p.District)
                   .WithMany(d => d.Parishes)
                   .HasForeignKey(p => p.DistrictId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

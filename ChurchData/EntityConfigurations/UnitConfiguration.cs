using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class UnitConfiguration : IEntityTypeConfiguration<Unit>
    {
        public void Configure(EntityTypeBuilder<Unit> builder)
        {
            builder.ToTable("units");

            builder.Property(u => u.UnitId).HasColumnName("unit_id");
            builder.Property(u => u.ParishId).HasColumnName("parish_id");
            builder.Property(u => u.UnitName)
                   .HasColumnName("unit_name")
                   .IsRequired()
                   .HasMaxLength(100);
            builder.Property(u => u.Description).HasColumnName("description");
            builder.Property(u => u.UnitPresident)
                   .HasColumnName("unit_president")
                   .HasMaxLength(100);
            builder.Property(u => u.UnitSecretary)
                   .HasColumnName("unit_secretary")
                   .HasMaxLength(100);

            builder.HasOne(u => u.Parish)
                   .WithMany(p => p.Units)
                   .HasForeignKey(u => u.ParishId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

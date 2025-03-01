using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace ChurchData.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles");

            builder.HasKey(r => r.Id); // Primary Key
            builder.Property(r => r.Id)
                   .HasColumnName("role_id")
                   .HasDefaultValueSql("uuid_generate_v4()"); // Fix identity issue

            builder.Property(r => r.Name)
                   .HasColumnName("role_name")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(r => r.NormalizedName)
                   .HasColumnName("normalized_name")
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(r => r.ConcurrencyStamp)
                   .HasColumnName("concurrencystamp")
                   .IsConcurrencyToken();

            // Ensure uniqueness for role_name and normalized_name
            builder.HasIndex(r => r.Name).IsUnique();
            builder.HasIndex(r => r.NormalizedName).IsUnique();
        }
    }
}

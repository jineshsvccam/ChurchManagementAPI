using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;

namespace ChurchData.EntityConfigurations
{
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("roles"); // Table name mapping

            builder.Property(r => r.Id)
                   .HasColumnName("role_id")
                   .UseIdentityAlwaysColumn();

            builder.Property(r => r.Name)
                   .HasColumnName("role_name")
                   .IsRequired()
                   .HasMaxLength(50);

            // Add required Identity fields
            builder.Property(r => r.NormalizedName)
                   .HasColumnName("normalized_name")
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(r => r.ConcurrencyStamp)
                   .HasColumnName("concurrencystamp")
                   .IsConcurrencyToken();
        }
    }
}

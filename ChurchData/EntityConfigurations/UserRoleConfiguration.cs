using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("user_roles");

            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.UserId)
                   .HasColumnName("user_id");

            builder.Property(ur => ur.RoleId)
                   .HasColumnName("role_id");

            builder.Property(ur => ur.Status)
                   .HasColumnName("status")
                   .HasConversion<string>() // Stores enum as string; adjust if mapping to PostgreSQL enum
                   .HasDefaultValue(RoleStatus.Pending);

            builder.Property(ur => ur.ApprovedBy)
                   .HasColumnName("approved_by")
                   .IsRequired(false);

            builder.Property(ur => ur.RequestedAt)
                   .HasColumnName("requested_at")
                   .HasColumnType("timestamp")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(ur => ur.ApprovedAt)
                   .HasColumnName("approved_at")
                   .HasColumnType("timestamp")
                   .IsRequired(false);

            builder.HasOne(ur => ur.User)
                   .WithMany(u => u.UserRoles)
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.Role)
                   .WithMany(r => r.UserRoles)
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(ur => ur.ApprovedByUser)
                   .WithMany()
                   .HasForeignKey(ur => ur.ApprovedBy)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(u => u.Id)
                   .HasColumnName("user_id")
                   .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(u => u.UserName)
                   .HasColumnName("username")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.FullName)
                   .HasColumnName("fullname")
                   .HasMaxLength(255)
                   .IsRequired();

            builder.Property(u => u.Mobile)
                   .HasColumnName("mobile")
                   .HasMaxLength(20)
                   .IsRequired();

            builder.Property(u => u.NormalizedUserName)
                   .HasColumnName("normalizedusername")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.NormalizedEmail)
                   .HasColumnName("normalizedemail")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.EmailConfirmed)
                   .HasColumnName("email_confirmed")
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .HasMaxLength(512)
                   .IsRequired();

            builder.Property(u => u.SecurityStamp)
                   .HasColumnName("securitystamp");

            builder.Property(u => u.ConcurrencyStamp)
                   .HasColumnName("concurrencystamp");

            builder.Property(u => u.TwoFactorEnabled)
                   .HasColumnName("twofactorenabled")
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(u => u.AccessFailedCount)
                   .HasColumnName("accessfailedcount")
                   .IsRequired()
                   .HasDefaultValue(0);

            builder.Property(u => u.LockoutEnabled)
                   .HasColumnName("lockoutenabled")
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(u => u.LockoutEnd)
                   .HasColumnName("lockoutend");

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(u => u.FamilyId)
                   .HasColumnName("family_id");

            builder.Property(u => u.ParishId)
                   .HasColumnName("parish_id");

            builder.Property(u => u.Status)
                   .HasColumnName("status")
                   .HasConversion<string>()
                   .HasDefaultValue(UserStatus.Pending);

            builder.HasOne(u => u.Family)
                   .WithMany(f => f.Users)
                   .HasForeignKey(u => u.FamilyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Parish)
                   .WithMany(p => p.Users)
                   .HasForeignKey(u => u.ParishId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}

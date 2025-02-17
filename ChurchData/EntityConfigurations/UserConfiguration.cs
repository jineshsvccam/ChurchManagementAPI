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
                   .UseIdentityAlwaysColumn();

            builder.Property(u => u.UserName)
                   .HasColumnName("username")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.NormalizedUserName)
                   .HasColumnName("normalizedusername")
                   .HasMaxLength(255);

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.NormalizedEmail)
                   .HasColumnName("normalizedemail")
                   .HasMaxLength(255);

            builder.Property(u => u.EmailConfirmed)
                   .HasColumnName("email_confirmed")
                   .IsRequired();

            builder.Property(u => u.PasswordHash)
                   .HasColumnName("password_hash")
                   .HasMaxLength(255);

            builder.Property(u => u.SecurityStamp)
                   .HasColumnName("securitystamp");

            builder.Property(u => u.ConcurrencyStamp)
                   .HasColumnName("concurrencystamp");

            builder.Property(u => u.PhoneNumber)
                   .HasColumnName("phonenumber")
                   .HasMaxLength(255);

            builder.Property(u => u.PhoneNumberConfirmed)
                   .HasColumnName("phonenumberconfirmed")
                   .IsRequired();

            builder.Property(u => u.TwoFactorEnabled)
                   .HasColumnName("twofactorenabled")
                   .IsRequired();

            builder.Property(u => u.AccessFailedCount)
                   .HasColumnName("accessfailedcount")
                   .IsRequired();

            builder.Property(u => u.LockoutEnabled)
                   .HasColumnName("lockoutenabled")
                   .IsRequired();

            builder.Property(u => u.LockoutEnd)
                   .HasColumnName("lockoutend");

            // Foreign Keys
            builder.Property(u => u.FamilyId)
                   .HasColumnName("family_id");

            builder.Property(u => u.ParishId)
                   .HasColumnName("parish_id");

            // User Status (Active, Inactive, Suspended)
            builder.Property(u => u.Status)
                   .HasColumnName("status")
                   .HasConversion<string>() // Enum stored as string
                   .HasDefaultValue(UserStatus.Pending);

            builder.HasOne(u => u.Family)
                   .WithMany(f => f.Users)
                   .HasForeignKey(u => u.FamilyId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(u => u.Parish)
                   .WithMany(p => p.Users)
                   .HasForeignKey(u => u.ParishId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

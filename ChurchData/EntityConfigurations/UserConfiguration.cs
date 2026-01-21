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

            builder.Property(u => u.PhoneNumber)
                    .HasColumnName("phonenumber")
                    .HasMaxLength(20);
                   

            builder.Property(u => u.PhoneNumberConfirmed)
                   .HasColumnName("phonenumberconfirmed")
                   .IsRequired()
                   .HasDefaultValue(false);

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
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                 .HasColumnType("timestamp with time zone")
                 .HasDefaultValueSql("CURRENT_TIMESTAMP AT TIME ZONE 'UTC'");

            builder.Property(u => u.FamilyId)
                   .HasColumnName("family_id");

            builder.Property(u => u.ParishId)
                   .HasColumnName("parish_id");

            builder.Property(u => u.Status)
                    .HasColumnName("status")
                    .HasColumnType("character varying(20)")
                    .HasConversion(
                        v => v.ToString(),
                        v => (UserStatus)Enum.Parse(typeof(UserStatus), v))
                    .HasDefaultValue(UserStatus.Pending)
                    .HasSentinel(UserStatus.Pending);

            builder.Property(u => u.TwoFactorType)
                   .HasColumnName("two_factor_type")
                   .HasMaxLength(20);

            builder.Property(u => u.TwoFactorEnabledAt)
                   .HasColumnName("two_factor_enabled_at")
                   .HasColumnType("timestamp with time zone");

            builder.HasOne(u => u.Family)
                   .WithMany(f => f.Users)
                   .HasForeignKey(u => u.FamilyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(u => u.Parish)
                   .WithMany(p => p.Users)
                   .HasForeignKey(u => u.ParishId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(u => u.TwoFactorType)
                   .HasDatabaseName("idx_users_two_factor_type");
        }
    }
}

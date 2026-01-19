using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class UserAuthenticatorConfiguration : IEntityTypeConfiguration<UserAuthenticator>
    {
        public void Configure(EntityTypeBuilder<UserAuthenticator> builder)
        {
            builder.ToTable("user_authenticators");

            builder.HasKey(e => e.AuthenticatorId);

            builder.Property(e => e.AuthenticatorId)
                .HasColumnName("authenticator_id")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.SecretKey)
                .HasColumnName("secret_key")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.IsActive)
                .HasColumnName("is_active")
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(e => e.VerifiedAt)
                .HasColumnName("verified_at")
                .HasColumnType("timestamp with time zone");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.RevokedAt)
                .HasColumnName("revoked_at")
                .HasColumnType("timestamp with time zone");

            builder.HasOne(e => e.User)
                .WithMany(u => u.Authenticators)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.UserId, e.IsActive })
                .HasDatabaseName("ux_user_authenticators_active")
                .IsUnique()
                .HasFilter("is_active = true");
        }
    }
}

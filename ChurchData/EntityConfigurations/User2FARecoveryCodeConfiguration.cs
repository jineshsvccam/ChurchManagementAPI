using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class User2FARecoveryCodeConfiguration : IEntityTypeConfiguration<User2FARecoveryCode>
    {
        public void Configure(EntityTypeBuilder<User2FARecoveryCode> builder)
        {
            builder.ToTable("user_2fa_recovery_codes");

            builder.HasKey(e => e.RecoveryCodeId);

            builder.Property(e => e.RecoveryCodeId)
                .HasColumnName("recovery_code_id")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.RecoveryCodeHash)
                .HasColumnName("recovery_code_hash")
                .HasMaxLength(256)
                .IsRequired();

            builder.Property(e => e.IsUsed)
                .HasColumnName("is_used")
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(e => e.UsedAt)
                .HasColumnName("used_at")
                .HasColumnType("timestamp with time zone");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.HasOne(e => e.User)
                .WithMany(u => u.RecoveryCodes)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.UserId)
                .HasDatabaseName("idx_recovery_codes_user");

            builder.HasIndex(e => new { e.UserId, e.IsUsed })
                .HasDatabaseName("idx_recovery_codes_unused");
        }
    }
}

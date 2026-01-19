using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class User2FASessionConfiguration : IEntityTypeConfiguration<User2FASession>
    {
        public void Configure(EntityTypeBuilder<User2FASession> builder)
        {
            builder.ToTable("user_2fa_sessions");

            builder.HasKey(e => e.SessionId);

            builder.Property(e => e.SessionId)
                .HasColumnName("session_id")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            builder.Property(e => e.TempToken)
                .HasColumnName("temp_token")
                .HasMaxLength(512)
                .IsRequired();

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45);

            builder.Property(e => e.UserAgent)
                .HasColumnName("user_agent");

            builder.Property(e => e.Attempts)
                .HasColumnName("attempts")
                .IsRequired()
                .HasDefaultValue((short)0);

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.ExpiresAt)
                .HasColumnName("expires_at")
                .HasColumnType("timestamp with time zone")
                .IsRequired();

            builder.HasOne(e => e.User)
                .WithMany(u => u.TwoFactorSessions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => e.TempToken)
                .HasDatabaseName("ux_2fa_sessions_token")
                .IsUnique();

            builder.HasIndex(e => e.ExpiresAt)
                .HasDatabaseName("idx_2fa_sessions_expiry");
        }
    }
}

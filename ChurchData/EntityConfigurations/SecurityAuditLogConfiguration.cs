using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class SecurityAuditLogConfiguration : IEntityTypeConfiguration<SecurityAuditLog>
    {
        public void Configure(EntityTypeBuilder<SecurityAuditLog> builder)
        {
            builder.ToTable("security_audit_logs");

            builder.HasKey(e => e.LogId);

            builder.Property(e => e.LogId)
                .HasColumnName("log_id")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(e => e.UserId)
                .HasColumnName("user_id");

            builder.Property(e => e.EventType)
                .HasColumnName("event_type")
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(e => e.EventDescription)
                .HasColumnName("event_description")
                .HasMaxLength(500);

            builder.Property(e => e.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45);

            builder.Property(e => e.UserAgent)
                .HasColumnName("user_agent")
                .HasMaxLength(512);

            // Configure Metadata as jsonb column type for PostgreSQL
            builder.Property(e => e.Metadata)
                .HasColumnName("metadata")
                .HasColumnType("jsonb");

            builder.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.Severity)
                .HasColumnName("severity")
                .HasMaxLength(20)
                .HasDefaultValue("Info");
        }
    }
}

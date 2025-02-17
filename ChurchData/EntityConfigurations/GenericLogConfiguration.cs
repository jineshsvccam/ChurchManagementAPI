using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace ChurchData.EntityConfigurations
{
    public class GenericLogConfiguration : IEntityTypeConfiguration<GenericLog>
    {
        public void Configure(EntityTypeBuilder<GenericLog> builder)
        {
            builder.ToTable("generic_logs");

            builder.HasKey(e => e.LogId);

            builder.Property(e => e.LogId).HasColumnName("log_id");
            builder.Property(e => e.TableName).HasColumnName("table_name");
            builder.Property(e => e.RecordId).HasColumnName("record_id");
            builder.Property(e => e.ChangeType).HasColumnName("change_type");
            builder.Property(e => e.ChangedBy).HasColumnName("changed_by");

            builder.Property(e => e.ChangeTimestamp)
                .HasColumnName("change_timestamp")
                .HasConversion(
                    v => v.ToUniversalTime(), // Ensure UTC when saving
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Ensure UTC when reading
                );

            builder.Property(e => e.OldValues).HasColumnName("old_values");
            builder.Property(e => e.NewValues).HasColumnName("new_values");
        }
    }
}

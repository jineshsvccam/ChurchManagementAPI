using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace ChurchData.EntityConfigurations
{
    public class ContributionSettingsConfiguration : IEntityTypeConfiguration<ContributionSettings>
    {
        public void Configure(EntityTypeBuilder<ContributionSettings> builder)
        {
            builder.ToTable("contribution_settings");

            builder.HasKey(cs => cs.SettingId);

            builder.Property(cs => cs.SettingId).HasColumnName("setting_id");
            builder.Property(cs => cs.HeadId).HasColumnName("head_id");
            builder.Property(cs => cs.ParishId).HasColumnName("parish_id");
            builder.Property(cs => cs.Amount).HasColumnName("amount").HasPrecision(15, 2);
            builder.Property(cs => cs.Frequency).HasColumnName("frequency").HasMaxLength(10);
            builder.Property(cs => cs.DueDay).HasColumnName("due_day");
            builder.Property(cs => cs.DueMonth).HasColumnName("due_month");
            builder.Property(cs => cs.FineAmount).HasColumnName("fine_amount").HasPrecision(15, 2);
            builder.Property(cs => cs.ValidFrom).HasColumnName("valid_from");
            builder.Property(cs => cs.Category).HasColumnName("category").HasMaxLength(10);

            builder.HasOne<TransactionHead>()
                   .WithMany()
                   .HasForeignKey(cs => cs.HeadId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Parish>()
                   .WithMany()
                   .HasForeignKey(cs => cs.ParishId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
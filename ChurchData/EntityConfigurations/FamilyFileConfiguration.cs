using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class FamilyFileConfiguration : IEntityTypeConfiguration<FamilyFile>
    {
        public void Configure(EntityTypeBuilder<FamilyFile> builder)
        {
            builder.ToTable("family_files");

            builder.HasKey(f => f.FileId);

            builder.Property(f => f.FileId)
                .HasColumnName("file_id");

            builder.Property(f => f.FamilyId)
                .HasColumnName("family_id")
                .IsRequired();

            builder.Property(f => f.MemberId)
                .HasColumnName("member_id");

            builder.Property(f => f.FileCategory)
                .HasColumnName("file_category")
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(f => f.FileType)
                .HasColumnName("file_type")
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(f => f.FileKey)
                .HasColumnName("file_key")
                .IsRequired();

            builder.Property(f => f.IsPrimary)
                .HasColumnName("is_primary")
                .HasDefaultValue(false);

            builder.Property(f => f.Status)
                .HasColumnName("status")
                .HasMaxLength(10)
                .HasDefaultValue("Pending");

            builder.Property(f => f.UploadedBy)
                .HasColumnName("uploaded_by");

            builder.Property(f => f.UploadedAt)
                .HasColumnName("uploaded_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // 🔗 Relationships
            builder.HasOne(f => f.Family)
                .WithMany()
                .HasForeignKey(f => f.FamilyId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(f => f.FamilyMember)
                .WithMany()
                .HasForeignKey(f => f.MemberId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

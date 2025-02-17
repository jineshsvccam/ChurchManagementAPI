using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class PendingFamilyMemberActionConfiguration : IEntityTypeConfiguration<PendingFamilyMemberAction>
    {
        public void Configure(EntityTypeBuilder<PendingFamilyMemberAction> builder)
        {
            builder.ToTable("pending_family_member_actions");

            builder.HasKey(e => e.ActionId)
                   .HasName("pending_family_member_actions_pkey");

            builder.Property(e => e.ActionId)
                   .HasColumnName("action_id")
                   .UseIdentityAlwaysColumn();

            builder.Property(e => e.FamilyId)
                   .HasColumnName("family_id");

            builder.Property(e => e.ParishId)
                   .HasColumnName("parish_id");

            builder.Property(e => e.MemberId)
                   .HasColumnName("member_id");

            builder.Property(e => e.SubmittedData)
                   .HasColumnName("submitted_data")
                   .HasColumnType("jsonb")
                   .IsRequired();

            builder.Property(e => e.ActionType)
                   .HasColumnName("action_type")
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(e => e.SubmittedBy)
                   .HasColumnName("submitted_by")
                   .IsRequired();

            builder.Property(e => e.SubmittedAt)
                   .HasColumnName("submitted_at")
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(e => e.ApprovalStatus)
                   .HasColumnName("approval_status")
                   .HasMaxLength(20)
                   .HasDefaultValue("Pending");

            builder.Property(e => e.ApprovedBy)
                   .HasColumnName("approved_by");

            builder.Property(e => e.ApprovedAt)
                   .HasColumnName("approved_at");

            // Relationships
            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.ApprovedBy)
                   .OnDelete(DeleteBehavior.SetNull)
                   .HasConstraintName("pending_family_member_actions_approved_by_fkey");

            builder.HasOne<Family>()
                   .WithMany()
                   .HasForeignKey(e => e.FamilyId)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("pending_family_member_actions_family_id_fkey");

            builder.HasOne<Parish>()
                   .WithMany()
                   .HasForeignKey(e => e.ParishId)
                   .OnDelete(DeleteBehavior.SetNull)
                   .HasConstraintName("pending_family_member_actions_parish_id_fkey");

            builder.HasOne<User>()
                   .WithMany()
                   .HasForeignKey(e => e.SubmittedBy)
                   .OnDelete(DeleteBehavior.Cascade)
                   .HasConstraintName("pending_family_member_actions_submitted_by_fkey");
        }
    }
}

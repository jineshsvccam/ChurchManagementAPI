using ChurchData.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData.EntityConfigurations
{
    public class RegistrationRequestConfiguration : IEntityTypeConfiguration<RegistrationRequest>
    {
        public void Configure(EntityTypeBuilder<RegistrationRequest> builder)
        {
            builder.ToTable("registration_requests");

            builder.HasKey(x => x.RequestId);
            builder.Property(x => x.RequestId)
                .HasColumnName("request_id")
                .HasDefaultValueSql("uuid_generate_v4()");

            builder.Property(x => x.FullName).HasColumnName("full_name");
            builder.Property(x => x.Email).HasColumnName("email");
            builder.Property(x => x.Role).HasColumnName("role");
            builder.Property(x => x.ParishId).HasColumnName("parish_id");
            builder.Property(x => x.FamilyId).HasColumnName("family_id");

            builder.Property(x => x.EmailVerificationToken).HasColumnName("email_verification_token");
            builder.Property(x => x.EmailVerified).HasColumnName("email_verified");
            builder.Property(x => x.EmailVerifiedAt).HasColumnName("email_verified_at");

            builder.Property(x => x.PhoneNumber).HasColumnName("phone_number");
            builder.Property(x => x.PhoneVerified).HasColumnName("phone_verified");
            builder.Property(x => x.PhoneVerifiedAt).HasColumnName("phone_verified_at");

            builder.Property(x => x.PhoneVerificationOtpHash).HasColumnName("phone_verification_otp_hash");
            builder.Property(x => x.PhoneOtpExpiresAt).HasColumnName("phone_otp_expires_at");
            builder.Property(x => x.PhoneOtpAttempts).HasColumnName("phone_otp_attempts");

            builder.Property(x => x.CreatedAt).HasColumnName("created_at");
            builder.Property(x => x.ExpiresAt).HasColumnName("expires_at");

            builder.HasIndex(x => x.Email).HasDatabaseName("idx_registration_requests_email");
            builder.HasIndex(x => x.EmailVerificationToken).IsUnique().HasDatabaseName("uq_registration_requests_token");
            builder.HasIndex(x => x.ExpiresAt).HasDatabaseName("idx_registration_requests_expires_at");
        }
    }
}

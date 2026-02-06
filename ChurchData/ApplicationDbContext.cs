using ChurchData.Entities;
using ChurchData.EntityConfigurations;
using ChurchDTOs.DTOs.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChurchData
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, Guid, IdentityUserClaim<Guid>, UserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Diocese> Dioceses { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<Parish> Parishes { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Family> Families { get; set; }
        public DbSet<TransactionHead> TransactionHeads { get; set; }
        public DbSet<Bank> Banks { get; set; }
        public DbSet<FamilyMember> FamilyMembers { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<FinancialReportsView> FinancialReportsView { get; set; }
        public DbSet<FinancialReportsViewDues> FinancialReportsViewDues { get; set; }

        public DbSet<BankDTO> BankBalances { get; set; }
        public DbSet<GenericLog> GenericLogs { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<PendingFamilyMemberAction> PendingFamilyMemberActions { get; set; }
        public DbSet<ContributionSettings> ContributionSettings { get; set; }
        public DbSet<FamilyDue> FamilyDues { get; set; }
        public DbSet<FamilyContribution> FamilyContributions { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<FamilyFile> FamilyFiles { get; set; }
        public DbSet<UserAuthenticator> UserAuthenticators { get; set; }
        public DbSet<User2FARecoveryCode> User2FARecoveryCodes { get; set; }
        public DbSet<User2FASession> User2FASessions { get; set; }
        public DbSet<SecurityAuditLog> SecurityAuditLogs { get; set; }
        public DbSet<MobileViewRequest> MobileViewRequests { get; set; }
        public DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }
        public DbSet<PhoneVerificationToken> PhoneVerificationTokens { get; set; }
        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
        public DbSet<RegistrationRequest> RegistrationRequests { get; set; }
        public DbSet<ParishPaymentMethod> ParishPaymentMethods { get; set; }
        public DbSet<MemberPayment> MemberPayments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Additionally, apply the consolidated FamilyMember-related mappings
            modelBuilder.ConfigureFamilyMemberMappings();

            // Map verification token entities to existing DB tables (PostgreSQL)
            modelBuilder.Entity<EmailVerificationToken>().ToTable("email_verification_tokens");
            modelBuilder.Entity<PhoneVerificationToken>().ToTable("phone_verification_tokens");
            modelBuilder.Entity<PasswordResetToken>().ToTable("password_reset_tokens");
        }
    }
}
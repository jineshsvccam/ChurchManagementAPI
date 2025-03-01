
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ChurchDTOs.DTOs.Utils;
using ChurchData.EntityConfigurations;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply all configurations from the current assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

            // Additionally, apply the consolidated FamilyMember-related mappings
            modelBuilder.ConfigureFamilyMemberMappings();
           
        }
    }
}
using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace ChurchData
{
    public class ApplicationDbContext : DbContext
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
        public DbSet<BankDTO> BankBalances { get; set; }
        public DbSet<GenericLog> GenericLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<FinancialYear> FinancialYears { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Diocese>(entity =>
            {
                entity.ToTable("dioceses");
                entity.HasKey(d => d.DioceseId);

                entity.Property(d => d.DioceseId).HasColumnName("diocese_id");
                entity.Property(d => d.DioceseName).HasColumnName("diocese_name");
                entity.Property(d => d.Address).HasColumnName("address");
                entity.Property(d => d.ContactInfo).HasColumnName("contact_info");
                entity.Property(d => d.Territory).HasColumnName("territory");

                entity.HasMany(d => d.Districts)
                        .WithOne(d => d.Diocese)
                        .HasForeignKey(d => d.DioceseId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<District>(entity =>
            {
                entity.ToTable("ecclesiastical_districts");
                entity.HasKey(d => d.DistrictId);

                entity.Property(d => d.DistrictId).HasColumnName("district_id");
                entity.Property(d => d.DistrictName).HasColumnName("district_name");
                entity.Property(d => d.DioceseId).HasColumnName("diocese_id");
                entity.Property(d => d.Description).HasColumnName("description");

                entity.HasOne(d => d.Diocese)
                      .WithMany(d => d.Districts)
                      .HasForeignKey(d => d.DioceseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(d => d.Parishes)
                      .WithOne(p => p.District)
                      .HasForeignKey(p => p.DistrictId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Parish>(entity =>
            {
                entity.ToTable("parishes");
                entity.Property(p => p.ParishId).HasColumnName("parish_id");
                entity.Property(p => p.ParishName).HasColumnName("parish_name");
                entity.Property(p => p.ParishLocation).HasColumnName("parish_location");
                entity.Property(p => p.Photo).HasColumnName("photo");
                entity.Property(p => p.Address).HasColumnName("address");
                entity.Property(p => p.Phone).HasColumnName("phone");
                entity.Property(p => p.Email).HasColumnName("email");
                entity.Property(p => p.Place).HasColumnName("place");
                entity.Property(p => p.Pincode).HasColumnName("pincode");
                entity.Property(p => p.VicarName).HasColumnName("vicar_name");
                entity.Property(p => p.DistrictId).HasColumnName("district_id");

                entity.HasOne(p => p.District)
                      .WithMany(d => d.Parishes)
                      .HasForeignKey(p => p.DistrictId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Unit>(entity =>
            {
                entity.ToTable("units");
                entity.Property(u => u.UnitId).HasColumnName("unit_id");
                entity.Property(u => u.ParishId).HasColumnName("parish_id");
                entity.Property(u => u.UnitName).HasColumnName("unit_name").IsRequired().HasMaxLength(100);
                entity.Property(u => u.Description).HasColumnName("description");
                entity.Property(u => u.UnitPresident).HasColumnName("unit_president").HasMaxLength(100);
                entity.Property(u => u.UnitSecretary).HasColumnName("unit_secretary").HasMaxLength(100);

                entity.HasOne(u => u.Parish)
                      .WithMany(p => p.Units)
                      .HasForeignKey(u => u.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Family>(entity =>
            {
                entity.ToTable("families");
                entity.Property(f => f.FamilyId).HasColumnName("family_id");
                entity.Property(f => f.UnitId).HasColumnName("unit_id");
                entity.Property(f => f.FamilyName).HasColumnName("family_name").IsRequired().HasMaxLength(100);
                entity.Property(f => f.Address).HasColumnName("address");
                entity.Property(f => f.ContactInfo).HasColumnName("contact_info").HasMaxLength(100);
                entity.Property(f => f.Category).HasColumnName("category").HasMaxLength(10);
                entity.Property(f => f.FamilyNumber).HasColumnName("family_number");
                entity.Property(f => f.Status).HasColumnName("status").HasMaxLength(10);
                entity.Property(f => f.HeadName).HasColumnName("head_name").IsRequired().HasMaxLength(50);
                entity.Property(f => f.ParishId).HasColumnName("parish_id").IsRequired();
                entity.Property(f => f.JoinDate).HasColumnName("join_date").HasColumnType("date");

                entity.HasOne(f => f.Unit)
                      .WithMany(u => u.Families) // Assuming Unit has a collection of Families
                      .HasForeignKey(f => f.UnitId)
                      .OnDelete(DeleteBehavior.Cascade);


            });

            modelBuilder.Entity<TransactionHead>(entity =>
            {
                entity.ToTable("transaction_heads", t =>
                {
                    t.HasCheckConstraint("transaction_head_type_check", "type IN ('Income', 'Expense', 'Both')");
                });
                entity.HasKey(t => t.HeadId);
                entity.Property(t => t.HeadId).HasColumnName("head_id");
                entity.Property(t => t.HeadName).HasColumnName("head_name").IsRequired().HasMaxLength(100);
                entity.Property(t => t.Type).HasColumnName("type").HasMaxLength(10);
                entity.Property(t => t.IsMandatory).HasColumnName("is_mandatory").HasDefaultValue(false);
                entity.Property(t => t.Description).HasColumnName("description");
                entity.Property(t => t.ParishId).HasColumnName("parish_id").IsRequired();
                entity.Property(t => t.Aramanapct).HasColumnName("aramanapct").HasColumnType("double precision");
                entity.Property(t => t.Ordr).HasColumnName("ordr").HasMaxLength(10);
                entity.Property(t => t.HeadNameMl).HasColumnName("head_name_ml").HasMaxLength(100);

                entity.HasOne(t => t.Parish)
                      .WithMany(p => p.TransactionHeads)
                      .HasForeignKey(t => t.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Bank>(entity =>
            {
                entity.ToTable("banks");
                entity.HasKey(b => b.BankId);
                entity.Property(b => b.BankId).HasColumnName("bank_id");
                entity.Property(b => b.BankName).HasColumnName("bank_name").IsRequired().HasMaxLength(100);
                entity.Property(b => b.AccountNumber).HasColumnName("account_number").HasMaxLength(50);
                entity.Property(b => b.OpeningBalance).HasColumnName("opening_balance").HasColumnType("numeric(15,2)").IsRequired();
                entity.Property(b => b.CurrentBalance).HasColumnName("current_balance").HasColumnType("numeric(15,2)").IsRequired();
                entity.Property(b => b.ParishId).HasColumnName("parish_id");

                entity.HasOne(b => b.Parish)
                      .WithMany(p => p.Banks)
                      .HasForeignKey(b => b.ParishId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<FamilyMember>(entity =>
            {
                entity.ToTable("family_members", t =>
                {
                    t.HasCheckConstraint("family_member_gender_check", "gender IN ('Male', 'Female', 'Other')");
                    t.HasCheckConstraint("family_member_role_check", "role IN ('Head', 'Member')");
                });
                entity.HasKey(fm => fm.MemberId);
                entity.Property(fm => fm.MemberId).HasColumnName("member_id");
                entity.Property(fm => fm.FamilyId).HasColumnName("family_id");
                entity.Property(fm => fm.FirstName).HasColumnName("first_name").IsRequired().HasMaxLength(100);
                entity.Property(fm => fm.LastName).HasColumnName("last_name").IsRequired().HasMaxLength(100);
                entity.Property(fm => fm.DateOfBirth).HasColumnName("date_of_birth").HasColumnType("date");
                entity.Property(fm => fm.Gender).HasColumnName("gender").HasMaxLength(10);
                entity.Property(fm => fm.ContactInfo).HasColumnName("contact_info").HasMaxLength(100);
                entity.Property(fm => fm.Role).HasColumnName("role").HasMaxLength(10);

                entity.HasOne(fm => fm.Family)
                      .WithMany(f => f.FamilyMembers)
                      .HasForeignKey(fm => fm.FamilyId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.ToTable("transactions", t =>
                {
                    t.HasCheckConstraint("transaction_expense_amount_check", "expense_amount >= 0");
                    t.HasCheckConstraint("transaction_income_amount_check", "income_amount >= 0");
                    t.HasCheckConstraint("transaction_type_check", "transaction_type IN ('Income', 'Expense')");
                });
                entity.HasKey(t => t.TransactionId);
                entity.Property(t => t.TransactionId).HasColumnName("transaction_id");
                entity.Property(t => t.TrDate).HasColumnName("tr_date").IsRequired().HasColumnType("date");
                entity.Property(t => t.VrNo).HasColumnName("vr_no").IsRequired().HasMaxLength(50);
                entity.Property(t => t.TransactionType).HasColumnName("transaction_type").HasMaxLength(10);
                entity.Property(t => t.HeadId).HasColumnName("head_id");
                entity.Property(t => t.FamilyId).HasColumnName("family_id");
                entity.Property(t => t.BankId).HasColumnName("bank_id");
                entity.Property(t => t.IncomeAmount).HasColumnName("income_amount").HasColumnType("numeric(15,2)").HasDefaultValue(0);
                entity.Property(t => t.ExpenseAmount).HasColumnName("expense_amount").HasColumnType("numeric(15,2)").HasDefaultValue(0);
                entity.Property(t => t.Description).HasColumnName("description").HasColumnType("text");
                entity.Property(t => t.ParishId).HasColumnName("parish_id").IsRequired();

                entity.HasOne(t => t.TransactionHead)
                                  .WithMany(th => th.Transactions)
                                  .HasForeignKey(t => t.HeadId)
                                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Family)
                                  .WithMany(f => f.Transactions)
                                  .HasForeignKey(t => t.FamilyId)
                                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Bank)
                                  .WithMany(b => b.Transactions)
                                  .HasForeignKey(t => t.BankId)
                                  .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Parish)
                                  .WithMany(p => p.Transactions)
                                  .HasForeignKey(t => t.ParishId)
                                  .OnDelete(DeleteBehavior.NoAction);
            });

            // Configure FinancialReportsView as a keyless entity
            modelBuilder.Entity<FinancialReportsView>(entity =>
            {
                entity.ToView("financialreportsview");
                entity.HasNoKey();

                entity.Property(e => e.TransactionId).HasColumnName("transactionid");
                entity.Property(e => e.TrDate).HasColumnName("trdate");
                entity.Property(e => e.VrNo).HasColumnName("vrno");
                entity.Property(e => e.TransactionType).HasColumnName("transactiontype");
                entity.Property(e => e.IncomeAmount).HasColumnName("incomeamount");
                entity.Property(e => e.ExpenseAmount).HasColumnName("expenseamount");
                entity.Property(e => e.Description).HasColumnName("description");
                entity.Property(e => e.ParishId).HasColumnName("parishid");
                entity.Property(e => e.ParishName).HasColumnName("parishname");
                entity.Property(e => e.FamilyId).HasColumnName("familyid");
                entity.Property(e => e.FamilyName).HasColumnName("familyname");
                entity.Property(e => e.HeadId).HasColumnName("headid");
                entity.Property(e => e.HeadName).HasColumnName("headname");
                entity.Property(e => e.BankId).HasColumnName("bankid");
                entity.Property(e => e.BankName).HasColumnName("bankname");
            });

            modelBuilder.Entity<BankDTO>(entity =>
            {
                entity.HasNoKey();
                entity.Property(b => b.BankName).HasColumnName("bank_name");
                entity.Property(b => b.OpeningBalance).HasColumnName("opening_balance");
                entity.Property(b => b.ClosingBalance).HasColumnName("closing_balance");
                entity.Property(b => b.Balance).HasColumnName("balance");
            });

            modelBuilder.Entity<GenericLog>(entity =>
            {
                entity.HasKey(e => e.LogId);
                entity.Property(e => e.LogId).HasColumnName("log_id"); 
                entity.Property(e => e.TableName).HasColumnName("table_name"); 
                entity.Property(e => e.RecordId).HasColumnName("record_id"); 
                entity.Property(e => e.ChangeType).HasColumnName("change_type"); 
                entity.Property(e => e.ChangedBy).HasColumnName("changed_by"); 
                entity.Property(e => e.ChangeTimestamp)
                      .HasColumnName("change_timestamp") 
                      .HasConversion(
                          v => v.ToUniversalTime(), // Ensure UTC when saving
                          v => DateTime.SpecifyKind(v, DateTimeKind.Utc) // Ensure UTC when reading
                      );
                entity.Property(e => e.OldValues).HasColumnName("old_values"); 
                entity.Property(e => e.NewValues).HasColumnName("new_values"); 
                entity.ToTable("generic_logs"); 
            });

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.Username).IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).IsRequired().HasMaxLength(255);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
               // entity.Property(u => u.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure the User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.HasKey(u => u.UserId);
                entity.Property(u => u.UserId).HasColumnName("user_id");
                entity.Property(u => u.Username).HasColumnName("username").IsRequired().HasMaxLength(255);
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash").IsRequired().HasMaxLength(255);
                entity.Property(u => u.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
                entity.Property(u => u.ParishId).HasColumnName("parish_id"); // Explicitly map ParishId
            //  entity.Property(u => u.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Configure the Role entity
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles");
                entity.HasKey(r => r.RoleId);
                entity.Property(r => r.RoleId).HasColumnName("role_id");
                entity.Property(r => r.RoleName).HasColumnName("role_name").IsRequired().HasMaxLength(50);
            });

            // Configure the UserRole entity
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.Property(ur => ur.UserId).HasColumnName("user_id");
                entity.Property(ur => ur.RoleId).HasColumnName("role_id");

                entity.HasOne(ur => ur.User)
                    .WithMany(u => u.UserRoles)
                    .HasForeignKey(ur => ur.UserId);

                entity.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId);
            });
            modelBuilder.Entity<FinancialYear>(entity =>
            {
                entity.ToTable("financial_years");
                entity.HasKey(fy => fy.FinancialYearId);
                entity.Property(fy => fy.FinancialYearId).HasColumnName("financial_year_id");
                entity.Property(fy => fy.ParishId).HasColumnName("parish_id").IsRequired();
                entity.Property(fy => fy.StartDate).HasColumnName("start_date").IsRequired();
                entity.Property(fy => fy.EndDate).HasColumnName("end_date").IsRequired();
                entity.Property(fy => fy.IsLocked).HasColumnName("is_locked").IsRequired().HasDefaultValue(false);
                entity.Property(fy => fy.LockDate).HasColumnName("lock_date");
                entity.Property(fy => fy.Description).HasColumnName("description");

                entity.HasOne(fy => fy.Parish)
                    .WithMany(p => p.FinancialYears)
                    .HasForeignKey(fy => fy.ParishId);
            });
        }
    }

}



using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace ChurchData
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
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
        public DbSet<FinancialYear> FinancialYears { get; set; }
        public DbSet<PendingFamilyMemberAction> PendingFamilyMemberActions { get; set; }
        public DbSet<ContributionSettings> ContributionSettings { get; set; }
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

            // Configure Identity entities (User, Role, UserRole)
           
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("users");
                entity.Property(u => u.Id).HasColumnName("user_id").UseIdentityAlwaysColumn();
                entity.Property(u => u.UserName).HasColumnName("username").HasMaxLength(100).IsRequired();
                entity.Property(u => u.NormalizedUserName).HasColumnName("normalizedusername").HasMaxLength(255);
                entity.Property(u => u.Email).HasColumnName("email").HasMaxLength(100).IsRequired();
                entity.Property(u => u.NormalizedEmail).HasColumnName("normalizedemail").HasMaxLength(255);
                entity.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed").IsRequired();
                entity.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(255);
                entity.Property(u => u.SecurityStamp).HasColumnName("securitystamp");
                entity.Property(u => u.ConcurrencyStamp).HasColumnName("concurrencystamp");
                entity.Property(u => u.PhoneNumber).HasColumnName("phonenumber").HasMaxLength(255);
                entity.Property(u => u.PhoneNumberConfirmed).HasColumnName("phonenumberconfirmed").IsRequired();
                entity.Property(u => u.TwoFactorEnabled).HasColumnName("twofactorenabled").IsRequired();
                entity.Property(u => u.AccessFailedCount).HasColumnName("accessfailedcount").IsRequired();
                entity.Property(u => u.LockoutEnabled).HasColumnName("lockoutenabled").IsRequired();
                entity.Property(u => u.LockoutEnd).HasColumnName("lockoutend");

                // Foreign Keys
                entity.Property(u => u.FamilyId).HasColumnName("family_id");
                entity.Property(u => u.ParishId).HasColumnName("parish_id");

                // User Status (Active, Inactive, Suspended)
                entity.Property(u => u.Status)
                      .HasColumnName("status")
                      .HasConversion<string>() // Enum stored as string
                      .HasDefaultValue(UserStatus.Pending);

                entity.HasOne(u => u.Family)
                      .WithMany(f => f.Users)
                      .HasForeignKey(u => u.FamilyId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(u => u.Parish)
                      .WithMany(p => p.Users)
                      .HasForeignKey(u => u.ParishId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Configure Role Table
            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("roles"); // Table name mapping
                entity.Property(r => r.Id).HasColumnName("role_id").UseIdentityAlwaysColumn();
                entity.Property(r => r.Name).HasColumnName("role_name").IsRequired().HasMaxLength(50);

                // Add required Identity fields
                entity.Property(r => r.NormalizedName).HasColumnName("normalized_name").HasMaxLength(50).IsRequired();
                entity.Property(r => r.ConcurrencyStamp).HasColumnName("concurrencystamp").IsConcurrencyToken();
            });

            // Configure UserRole Table
            // Configure UserRole Table
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("user_roles");
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });

                entity.Property(ur => ur.UserId).HasColumnName("user_id");
                entity.Property(ur => ur.RoleId).HasColumnName("role_id");

                // Enum-based column for Role Status
                entity.Property(ur => ur.Status)
                      .HasColumnName("status")
                      .HasConversion<string>() // Store as string in DB
                      .HasDefaultValue(RoleStatus.Pending);

                // Track who approved the role assignment
                entity.Property(ur => ur.ApprovedBy)
                      .HasColumnName("approved_by")
                      .IsRequired(false);

                // Timestamp fields
                entity.Property(ur => ur.RequestedAt)
                      .HasColumnName("requested_at")
                      .HasColumnType("timestamp")
                      .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(ur => ur.ApprovedAt)
                      .HasColumnName("approved_at")
                      .HasColumnType("timestamp")
                      .IsRequired(false);

                // Relationships
                entity.HasOne(ur => ur.User)
                      .WithMany(u => u.UserRoles)
                      .HasForeignKey(ur => ur.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                      .WithMany(r => r.UserRoles)
                      .HasForeignKey(ur => ur.RoleId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Relationship with ApprovedBy (Admin)
                entity.HasOne(ur => ur.ApprovedByUser)
                      .WithMany()
                      .HasForeignKey(ur => ur.ApprovedBy)
                      .OnDelete(DeleteBehavior.SetNull);
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

            modelBuilder.Entity<PendingFamilyMemberAction>(entity =>
            {
                entity.ToTable("pending_family_member_actions");
                entity.HasKey(e => e.ActionId).HasName("pending_family_member_actions_pkey");
                entity.Property(e => e.ActionId).HasColumnName("action_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.FamilyId).HasColumnName("family_id");
                entity.Property(e => e.ParishId).HasColumnName("parish_id");               
                entity.Property(e => e.MemberId).HasColumnName("member_id");
                entity.Property(e => e.SubmittedData).HasColumnName("submitted_data").HasColumnType("jsonb").IsRequired();
                entity.Property(e => e.ActionType).HasColumnName("action_type").HasMaxLength(10).IsRequired();
                entity.Property(e => e.SubmittedBy).HasColumnName("submitted_by").IsRequired();
                entity.Property(e => e.SubmittedAt).HasColumnName("submitted_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.ApprovalStatus).HasColumnName("approval_status").HasMaxLength(20).HasDefaultValue("Pending");
                entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
                entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");

                entity.HasOne<User>().WithMany().HasForeignKey(e => e.ApprovedBy).OnDelete(DeleteBehavior.SetNull).HasConstraintName("pending_family_member_actions_approved_by_fkey");
                entity.HasOne<Family>().WithMany().HasForeignKey(e => e.FamilyId).OnDelete(DeleteBehavior.Cascade).HasConstraintName("pending_family_member_actions_family_id_fkey");
                entity.HasOne<Parish>().WithMany().HasForeignKey(e => e.ParishId).OnDelete(DeleteBehavior.SetNull).HasConstraintName("pending_family_member_actions_parish_id_fkey");
                entity.HasOne<User>().WithMany().HasForeignKey(e => e.SubmittedBy).OnDelete(DeleteBehavior.Cascade).HasConstraintName("pending_family_member_actions_submitted_by_fkey");
            });
            // FamilyMembers mapping
            modelBuilder.Entity<FamilyMember>(entity =>
            {
                entity.ToTable("family_members");
                entity.HasKey(e => e.MemberId);
                entity.Property(e => e.MemberId).HasColumnName("member_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.FamilyId).HasColumnName("family_id").IsRequired();
                entity.Property(e => e.ParishId).HasColumnName("parish_id");
                entity.Property(e => e.UnitId).HasColumnName("unit_id");
                entity.Property(e => e.FamilyNumber).HasColumnName("family_number").IsRequired();
                entity.Property(e => e.FirstName).HasColumnName("first_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.LastName).HasColumnName("last_name").HasMaxLength(100).IsRequired();
                entity.Property(e => e.Nickname).HasColumnName("nickname").HasMaxLength(100);
                entity.Property(e => e.Gender).HasColumnName("gender").HasConversion<string>().IsRequired();
                entity.Property(e => e.DateOfBirth).HasColumnName("date_of_birth");
                entity.Property(e => e.Age).HasColumnName("age");
                entity.Property(e => e.MaritalStatus).HasColumnName("marital_status").HasConversion<string>();
                entity.Property(e => e.ActiveMember).HasColumnName("active_member").HasDefaultValue(true);
                entity.Property(e => e.MemberStatus).HasColumnName("member_status").HasConversion<string>();
                entity.Property(e => e.UpdatedUser).HasColumnName("updated_user");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.UpdatedAt).HasColumnName("updated_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
                //entity.HasOne(e => e.Family).WithMany(f => f.Members).HasForeignKey(e => e.FamilyId).OnDelete(DeleteBehavior.Cascade);
                //entity.HasOne(e => e.Parish).WithMany(p => p.Members).HasForeignKey(e => e.ParishId).OnDelete(DeleteBehavior.SetNull);
                //entity.HasOne(e => e.Unit).WithMany(u => u.Members).HasForeignKey(e => e.UnitId).OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(e => e.UpdatedUserNavigation).WithMany().HasForeignKey(e => e.UpdatedUser).OnDelete(DeleteBehavior.SetNull);
               // entity.HasMany(e => e.Relations).WithOne(r => r.Member).HasForeignKey(r => r.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            // FamilyMemberContacts mapping
            modelBuilder.Entity<FamilyMemberContacts>(entity =>
            {
                entity.ToTable("family_member_contacts");
                entity.HasKey(e => e.ContactId);
                entity.Property(e => e.ContactId).HasColumnName("contact_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.AddressLine2).HasColumnName("address_line2").HasMaxLength(255);
                entity.Property(e => e.AddressLine3).HasColumnName("address_line3").HasMaxLength(255);
                entity.Property(e => e.PostOffice).HasColumnName("post_office").HasMaxLength(100);
                entity.Property(e => e.PinCode).HasColumnName("pin_code").HasMaxLength(10);
                entity.Property(e => e.LandPhone).HasColumnName("land_phone").HasMaxLength(20);
                entity.Property(e => e.MobilePhone).HasColumnName("mobile_phone").HasMaxLength(20);
                entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255);
                entity.Property(e => e.FacebookProfile).HasColumnName("facebook_profile").HasMaxLength(255);
                entity.Property(e => e.GeoLocation).HasColumnName("geo_location").HasMaxLength(255);
                entity.HasOne(e => e.Member).WithMany(m => m.Contacts).HasForeignKey(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            //// FamilyMemberIdentity mapping
            modelBuilder.Entity<FamilyMemberIdentity>(entity =>
            {
                entity.ToTable("family_member_identity");
                entity.HasKey(e => e.IdentityId);
                entity.Property(e => e.IdentityId).HasColumnName("identity_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.AadharNumber).HasColumnName("aadhar_number").HasMaxLength(20);
                entity.Property(e => e.PassportNumber).HasColumnName("passport_number").HasMaxLength(20);
                entity.Property(e => e.DrivingLicense).HasColumnName("driving_license").HasMaxLength(20);
                entity.Property(e => e.VoterId).HasColumnName("voter_id").HasMaxLength(20);
                entity.HasOne(e => e.Member).WithOne(m => m.Identity).HasForeignKey<FamilyMemberIdentity>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            // FamilyMemberOccupation mapping
            modelBuilder.Entity<FamilyMemberOccupation>(entity =>
            {
                entity.ToTable("family_member_occupation");
                entity.HasKey(e => e.OccupationId);
                entity.Property(e => e.OccupationId).HasColumnName("occupation_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.Qualification).HasColumnName("qualification").HasMaxLength(255);
                entity.Property(e => e.StudentOrEmployee)
                      .HasColumnName("student_or_employee")
                      .HasConversion<string>()  // This converts the enum to string
                      .HasMaxLength(10);
                entity.Property(e => e.ClassOrWork).HasColumnName("class_or_work").HasMaxLength(255);
                entity.Property(e => e.SchoolOrWorkplace).HasColumnName("school_or_workplace").HasMaxLength(255);
                entity.Property(e => e.SundaySchoolClass).HasColumnName("sunday_school_class").HasMaxLength(50);
                entity.HasOne(e => e.Member).WithOne(m => m.Occupation).HasForeignKey<FamilyMemberOccupation>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });


            // FamilyMemberSacraments mapping
            modelBuilder.Entity<FamilyMemberSacraments>(entity =>
            {
                entity.ToTable("family_member_sacraments");
                entity.HasKey(e => e.SacramentId);
                entity.Property(e => e.SacramentId).HasColumnName("sacrament_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.BaptismalName).HasColumnName("baptismal_name").HasMaxLength(255);
                entity.Property(e => e.BaptismDate).HasColumnName("baptism_date");
                entity.Property(e => e.MarriageDate).HasColumnName("marriage_date");
                entity.Property(e => e.MooronDate).HasColumnName("mooron_date");
                entity.Property(e => e.MooronInOurChurch).HasColumnName("mooron_in_our_church").HasDefaultValue(false);
                entity.Property(e => e.MarriageInOurChurch).HasColumnName("marriage_in_our_church").HasDefaultValue(false);
                entity.Property(e => e.BaptismInOurChurch).HasColumnName("baptism_in_our_church").HasDefaultValue(false);
                entity.HasOne(e => e.Member).WithOne(m => m.Sacraments).HasForeignKey<FamilyMemberSacraments>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            // FamilyMemberRelations mapping
            modelBuilder.Entity<FamilyMemberRelations>(entity =>
            {
                entity.ToTable("family_member_relations");
                entity.HasKey(e => e.RelationId);
                entity.Property(e => e.RelationId).HasColumnName("relation_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.FatherName).HasColumnName("father_name").HasMaxLength(100);
                entity.Property(e => e.MotherName).HasColumnName("mother_name").HasMaxLength(100);
                entity.Property(e => e.SpouseId).HasColumnName("spouse_id");
                entity.Property(e => e.ParentId).HasColumnName("parent_id");

                // Main relationship: FamilyMember -> Relations using MemberId
                entity.HasOne(r => r.Member)
                      .WithMany(m => m.Relations)
                      .HasForeignKey(r => r.MemberId)
                      .OnDelete(DeleteBehavior.Cascade);

                // For Spouse and Parent, we do not set an inverse navigation (i.e. WithMany() without parameter)
                entity.HasOne(r => r.Spouse)
                      .WithMany() // no inverse navigation on FamilyMember
                      .HasForeignKey(r => r.SpouseId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(r => r.Parent)
                      .WithMany() // no inverse navigation on FamilyMember
                      .HasForeignKey(r => r.ParentId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


            // FamilyMemberFiles mapping
            modelBuilder.Entity<FamilyMemberFiles>(entity =>
            {
                entity.ToTable("family_member_files");
                entity.HasKey(e => e.FileId);
                entity.Property(e => e.FileId).HasColumnName("file_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.MarriageFileNo).HasColumnName("marriage_file_no").HasMaxLength(50);
                entity.Property(e => e.BaptismFileNo).HasColumnName("baptism_file_no").HasMaxLength(50);
                entity.Property(e => e.DeathFileNo).HasColumnName("death_file_no").HasMaxLength(50);
                entity.Property(e => e.JoinFileNo).HasColumnName("join_file_no").HasMaxLength(50);
                entity.Property(e => e.MooronFileNo).HasColumnName("mooron_file_no").HasMaxLength(50);
                entity.Property(e => e.CommonCellNo).HasColumnName("common_cell_no").HasMaxLength(50);
                entity.HasOne(e => e.Member).WithOne(m => m.Files).HasForeignKey<FamilyMemberFiles>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

            // FamilyMemberLifecycle mapping
            modelBuilder.Entity<FamilyMemberLifecycle>(entity =>
            {
                entity.ToTable("family_member_lifecycle");
                entity.HasKey(e => e.LifecycleId);
                entity.Property(e => e.LifecycleId).HasColumnName("lifecycle_id").UseIdentityAlwaysColumn();
                entity.Property(e => e.MemberId).HasColumnName("member_id").IsRequired();
                entity.Property(e => e.CommonCell).HasColumnName("common_cell").HasDefaultValue(false);
                entity.Property(e => e.LeftReason).HasColumnName("left_reason");
                entity.Property(e => e.JoinDate).HasColumnName("join_date");
                entity.Property(e => e.LeftDate).HasColumnName("left_date");
                entity.Property(e => e.BurialPlace).HasColumnName("burial_place").HasMaxLength(20);
                entity.Property(e => e.DeathDate).HasColumnName("death_date");
                entity.HasOne(e => e.Member).WithOne(m => m.Lifecycle).HasForeignKey<FamilyMemberLifecycle>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<ContributionSettings>(entity =>
            {
                entity.ToTable("contribution_settings");

                entity.HasKey(cs => cs.SettingId);

                entity.Property(cs => cs.SettingId).HasColumnName("setting_id");
                entity.Property(cs => cs.HeadId).HasColumnName("head_id");
                entity.Property(cs => cs.ParishId).HasColumnName("parish_id");
                entity.Property(cs => cs.Amount).HasColumnName("amount").HasPrecision(15, 2);
                entity.Property(cs => cs.Frequency).HasColumnName("frequency").HasMaxLength(10);
                entity.Property(cs => cs.DueDay).HasColumnName("due_day");
                entity.Property(cs => cs.DueMonth).HasColumnName("due_month");
                entity.Property(cs => cs.FineAmount).HasColumnName("fine_amount").HasPrecision(15, 2);
                entity.Property(cs => cs.ValidFrom).HasColumnName("valid_from");
                entity.Property(cs => cs.Category).HasColumnName("category").HasMaxLength(10);

                entity.HasOne<TransactionHead>()
                      .WithMany()
                      .HasForeignKey(cs => cs.HeadId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<Parish>()
                      .WithMany()
                      .HasForeignKey(cs => cs.ParishId)
                      .OnDelete(DeleteBehavior.Restrict);
            });


        }
    }
}
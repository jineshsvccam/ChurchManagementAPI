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
        }
    }

}



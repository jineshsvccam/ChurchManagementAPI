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

        }
    }

}



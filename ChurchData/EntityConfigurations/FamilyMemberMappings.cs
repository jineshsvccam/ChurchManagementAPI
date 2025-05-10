using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ChurchData.EntityConfigurations
{
    public static class FamilyMemberMappings
    {
        public static void ConfigureFamilyMemberMappings(this ModelBuilder modelBuilder)
        {
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
                entity.Property(e => e.BurialPlace).HasColumnName("burial_place").HasMaxLength(20).HasConversion<string>(); 
                entity.Property(e => e.DeathDate).HasColumnName("death_date");
                entity.HasOne(e => e.Member).WithOne(m => m.Lifecycle).HasForeignKey<FamilyMemberLifecycle>(e => e.MemberId).OnDelete(DeleteBehavior.Cascade);
            });

        }
    }

}

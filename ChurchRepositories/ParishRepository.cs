using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ChurchRepositories
{
    public class ParishRepository : IParishRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ParishRepository(ApplicationDbContext context, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        private async Task<User> GetCurrentUserAsync()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                throw new InvalidOperationException("Invalid user ID format.");

            return await _userManager.FindByIdAsync(userId.ToString()) ?? throw new KeyNotFoundException("User not found.");
        }

        public async Task<IEnumerable<Parish>> GetAllAsync()
        {
            return await _context.Parishes.ToListAsync();           
        }

        public async Task<Parish?> GetByIdAsync(int id)
        {
           // var user = await GetCurrentUserAsync();
            var parish = await _context.Parishes.FindAsync(id);

            //if (parish?.ParishId != user.ParishId)
            //    throw new UnauthorizedAccessException("You are not authorized to access this parish data.");

            return parish;
        }

        public async Task<Parish> AddAsync(Parish parish)
        {
            await _context.Parishes.AddAsync(parish);
            await _context.SaveChangesAsync();
            return parish;
        }

        public async Task UpdateAsync(Parish parish)
        {
            var existingParish = await _context.Parishes.FindAsync(parish.ParishId);
            if (existingParish == null)
                throw new KeyNotFoundException("Parish not found.");

            _context.Entry(existingParish).CurrentValues.SetValues(parish);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var parish = await _context.Parishes.FindAsync(id);
            if (parish == null)
                throw new KeyNotFoundException("Parish not found.");

            _context.Parishes.Remove(parish);
            await _context.SaveChangesAsync();
        }

        public async Task<ParishDetailsDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false)
        {
            var user = await GetCurrentUserAsync();
            if (parishId != user.ParishId)
                throw new UnauthorizedAccessException("You are not authorized to access this parish data.");

            var parish = await _context.Parishes.FindAsync(parishId) ?? throw new KeyNotFoundException("Parish not found.");

            var details = new ParishDetailsDto
            {
                ParishId = parish.ParishId,
                ParishName = parish.ParishName,
                ParishLocation = parish.ParishLocation,
                Photo = parish.Photo,
                Address = parish.Address,
                Phone = parish.Phone,
                Email = parish.Email,
                Place = parish.Place,
                Pincode = parish.Pincode,
                VicarName = parish.VicarName,
                DistrictId = parish.DistrictId,
                Units = await _context.Units.Where(u => u.ParishId == parishId).Select(u => new UnitDto
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
                    Description = u.Description,
                    UnitPresident = u.UnitPresident,
                    UnitSecretary = u.UnitSecretary
                }).ToListAsync(),
                Families = await _context.Families.Where(f => f.ParishId == parishId).Select(f => new FamilyDto
                {
                    FamilyId = f.FamilyId,
                    FamilyName = f.FamilyName,
                    Address = f.Address,
                    ContactInfo = f.ContactInfo,
                    Category = f.Category,
                    FamilyNumber = f.FamilyNumber,
                    Status = f.Status,
                    HeadName = f.HeadName,
                    JoinDate = f.JoinDate ?? DateTime.MinValue
                }).ToListAsync(),
                TransactionHeads = await _context.TransactionHeads.Where(th => th.ParishId == parishId).Select(th => new TransactionHeadDto
                {
                    HeadId = th.HeadId,
                    HeadName = th.HeadName,
                    Type = th.Type,
                    Description = th.Description
                }).ToListAsync(),
                Banks = await _context.Banks.Where(b => b.ParishId == parishId).Select(b => new BankDto
                {
                    BankId = b.BankId,
                    BankName = b.BankName,
                    AccountNumber = b.AccountNumber,
                    OpeningBalance = b.OpeningBalance,
                    CurrentBalance = b.CurrentBalance
                }).ToListAsync(),
                FinancialYears = await _context.FinancialYears.Where(fy => fy.ParishId == parishId).Select(fy => new FinancialYearDto
                {
                    FinancialYearId = fy.FinancialYearId,
                    StartDate = fy.StartDate,
                    EndDate = fy.EndDate,
                    IsLocked = fy.IsLocked,
                    LockDate = fy.LockDate,
                    Description = fy.Description
                }).ToListAsync(),
                FamilyMembers = includeFamilyMembers ? await _context.FamilyMembers.Where(fm => _context.Families.Where(f => f.ParishId == parishId).Select(f => f.FamilyId).Contains(fm.FamilyId)).Select(fm => new FamilyMemberDto
                {
                    FamilyId = fm.FamilyId,
                    UnitId = fm.Family.UnitId,
                    MemberId = fm.MemberId,
                    FirstName = fm.FirstName,
                    LastName = fm.LastName,
                    DateOfBirth = fm.DateOfBirth,
                    Gender = fm.Gender,
                    ContactInfo = fm.ContactInfo,
                    Role = fm.Role
                }).ToListAsync() : new List<FamilyMemberDto>()
            };

            return details;
        }
    }
}

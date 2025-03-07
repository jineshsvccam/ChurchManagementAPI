using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using ChurchDTOs.DTOs.Entities;

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

        public async Task<ParishDetailsBasicDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false)
        {
            //var user = await GetCurrentUserAsync();
            //if (parishId != user.ParishId)
            //    throw new UnauthorizedAccessException("You are not authorized to access this parish data.");

            var parish = await _context.Parishes.FindAsync(parishId) ?? throw new KeyNotFoundException("Parish not found.");

            var details = new ParishDetailsBasicDto
            {
                ParishId = parish.ParishId,
                ParishName = parish.ParishName,

                Units = await _context.Units.Where(u => u.ParishId == parishId).Select(u => new UnitBasicDto
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName

                }).ToListAsync(),
                Families = await _context.Families.Where(f => f.ParishId == parishId).Select(f => new FamilyBasicDto
                {
                    FamilyId = f.FamilyId,
                    FamilyName = string.Concat(f.HeadName, " ", f.FamilyName),
                    FamilyNumber = f.FamilyNumber,
                    UnitId=f.UnitId

                }).ToListAsync(),
                TransactionHeads = await _context.TransactionHeads.Where(th => th.ParishId == parishId).Select(th => new TransactionHeadBasicDto
                {
                    HeadId = th.HeadId,
                    HeadName = th.HeadName,
                    Type = th.Type,
                    Aramanapct = th.Aramanapct,
                    Ordr = th.Ordr
                }).ToListAsync(),
                Banks = await _context.Banks.Where(b => b.ParishId == parishId).Select(b => new BankBasicDto
                {
                    BankId = b.BankId,
                    BankName = b.BankName
                }).ToListAsync(),
                FinancialYears = await _context.FinancialYears.Where(fy => fy.ParishId == parishId).Select(fy => new FinancialYearBasicDto
                {
                    FinancialYearId = fy.FinancialYearId,
                    StartDate = fy.StartDate,
                    EndDate = fy.EndDate,
                    IsLocked = fy.IsLocked

                }).ToListAsync(),
                FamilyMembers = includeFamilyMembers ? await _context.FamilyMembers.Where(fm => _context.Families.Where(f => f.ParishId == parishId).Select(f => f.FamilyId).Contains(fm.FamilyId)).Select(fm => new FamilyMemberDto
                {
                    FamilyId = fm.FamilyId,
                    UnitId = (int)fm.UnitId,
                    MemberId = fm.MemberId,
                    FirstName = fm.FirstName,
                    LastName = fm.LastName,
                    DateOfBirth = fm.DateOfBirth,
                    Gender = fm.Gender.ToString()
                }).ToListAsync() : new List<FamilyMemberDto>()
            };

            return details;
        }
    }
}

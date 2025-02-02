using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories
{
    public class ParishRepository : IParishRepository
    {
        private readonly ApplicationDbContext _context;

        public ParishRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Parish>> GetAllAsync()
        {
            return await _context.Parishes.ToListAsync();
        }

        public async Task<Parish?> GetByIdAsync(int id)
        {
            return await _context.Parishes.FindAsync(id);
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
            if (existingParish != null)
            {
                _context.Entry(existingParish).CurrentValues.SetValues(parish);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Parish not found");
            }
        }

        public async Task DeleteAsync(int id)
        {
            var parish = await _context.Parishes.FindAsync(id);
            if (parish != null)
            {
                _context.Parishes.Remove(parish);
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new KeyNotFoundException("Parish not found");
            }
        }
        public async Task<ParishDetailsDto> GetParishDetailsAsync(int parishId, bool includeTransactions = false, bool includeFamilyMembers = false)
        {
            var query = _context.Parishes
                .Include(p => p.Units)
                .Include(p => p.Families)
                .Include(p => p.TransactionHeads)
                .Include(p => p.Banks)
                .AsQueryable();

            if (includeTransactions)
            {
                query = query.Include(p => p.Transactions);
            }

            if (includeFamilyMembers)
            {
                query = query.Include(p => p.Families)
                             .ThenInclude(f => f.FamilyMembers);
            }

            var parish = await query.FirstOrDefaultAsync(p => p.ParishId == parishId);

            if (parish == null)
            {
                throw new KeyNotFoundException("Parish not found");
            }

            return new ParishDetailsDto
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

                Units = parish.Units?.Select(u => new UnitDto
                {
                    UnitId = u.UnitId,
                    UnitName = u.UnitName,
                    Description = u.Description,
                    UnitPresident = u.UnitPresident,
                    UnitSecretary = u.UnitSecretary
                }).ToList() ?? new List<UnitDto>(),

                Families = parish.Families?.Select(f => new FamilyDto
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
                }).ToList() ?? new List<FamilyDto>(),

                TransactionHeads = parish.TransactionHeads?.Select(th => new TransactionHeadDto
                {
                    HeadId = th.HeadId,
                    HeadName = th.HeadName,
                    Type = th.Type,
                    Description = th.Description
                }).ToList() ?? new List<TransactionHeadDto>(),

                Banks = parish.Banks?.Select(b => new BankDto
                {
                    BankId = b.BankId,
                    BankName = b.BankName,
                    AccountNumber = b.AccountNumber,
                    OpeningBalance = b.OpeningBalance,
                    CurrentBalance = b.CurrentBalance
                }).ToList() ?? new List<BankDto>(),

                Transactions = includeTransactions ? (parish.Transactions?.Select(t => new TransactionDto
                {
                    TransactionId = t.TransactionId,
                    TrDate = t.TrDate,
                    VrNo = t.VrNo,
                    TransactionType = t.TransactionType,
                    IncomeAmount = t.IncomeAmount,
                    ExpenseAmount = t.ExpenseAmount,
                    Description = t.Description,
                    HeadId = t.HeadId,
                    FamilyId = t.FamilyId,
                    BankId = t.BankId

                    //HeadName=t.TransactionHead?.HeadName,
                    //FamilyName = t.Family?.FamilyName,
                    //BankName = t.Bank?.BankName

                }).ToList() ?? new List<TransactionDto>()) : new List<TransactionDto>(),

                FamilyMembers = includeFamilyMembers ? (parish.Families?.SelectMany(f => f.FamilyMembers).Select(fm => new FamilyMemberDto
                {
                    FamilyId = fm.FamilyId,
                    UnitId = fm.Family?.UnitId ?? 0, 
                    MemberId = fm.MemberId,
                    FirstName = fm.FirstName,
                    LastName = fm.LastName,
                    DateOfBirth = fm.DateOfBirth,
                    Gender = fm.Gender,
                    ContactInfo = fm.ContactInfo,
                    Role = fm.Role
                }).ToList() ?? new List<FamilyMemberDto>()) : new List<FamilyMemberDto>()
            };
        }
    }
}



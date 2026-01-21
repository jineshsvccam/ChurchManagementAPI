using ChurchCommon.Utils;
using ChurchContracts.ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ChurchRepositories.Settings
{
    public class BankRepository : IBankRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly LogsHelper _logsHelper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BankRepository(ApplicationDbContext context,
                              IHttpContextAccessor httpContextAccessor,
                              LogsHelper logsHelper)
        {
            _context = context;
            _logsHelper = logsHelper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IEnumerable<Bank>> GetBanksAsync(int? parishId, int? bankId)
        {
            var query = _context.Banks.AsNoTracking().AsQueryable();

            if (parishId.HasValue)
            {
                query = query.Where(b => b.ParishId == parishId.Value);
            }

            if (bankId.HasValue)
            {
                query = query.Where(b => b.BankId == bankId.Value);
            }

            return await query.ToListAsync();
        }

        public async Task<Bank?> GetByIdAsync(int id)
        {
            return await _context.Banks.FindAsync(id);
        }

        public async Task<Bank> AddAsync(Bank bank)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, bank.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            await _context.Banks.AddAsync(bank);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("banks", bank.BankId, "INSERT", userId, null, Extensions.Serialize(bank));
            return bank;
        }

        public async Task<Bank> UpdateAsync(Bank bank)
        {
            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, bank.ParishId);

            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var existingBank = await _context.Banks.FindAsync(bank.BankId);
            if (existingBank == null)
            {
                throw new KeyNotFoundException("Bank not found");
            }

            var oldValues = existingBank.Clone();
            _context.Entry(existingBank).CurrentValues.SetValues(bank);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("banks", bank.BankId, "UPDATE", userId, Extensions.Serialize(oldValues), Extensions.Serialize(bank));
            return bank;
        }

        public async Task DeleteAsync(int id)
        {
            int userId = UserHelper.GetCurrentUserId(_httpContextAccessor);
            var bank = await _context.Banks.FindAsync(id);
            if (bank == null)
            {
                throw new KeyNotFoundException("Bank not found");
            }

            await UserHelper.ValidateParishOwnershipAsync(_httpContextAccessor, _context, bank.ParishId);

            _context.Banks.Remove(bank);
            await _context.SaveChangesAsync();
            await _logsHelper.LogChangeAsync("banks", bank.BankId, "DELETE", userId, Extensions.Serialize(bank), null);
        }
    }
}

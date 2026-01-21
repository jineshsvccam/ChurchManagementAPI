using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchRepositories.Admin
{
    public class DioceseRepository : IDioceseRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DioceseRepository> _logger;

        public DioceseRepository(ApplicationDbContext context, ILogger<DioceseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Diocese>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all dioceses from database.");
            try
            {
                var dioceses = await _context.Dioceses.ToListAsync();
                _logger.LogInformation("Fetched {Count} dioceses.", dioceses.Count());
                return dioceses;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching dioceses.");
                throw;
            }
        }

        public async Task<Diocese> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching diocese with ID: {Id}", id);
            try
            {
                var diocese = await _context.Dioceses
                    .Include(d => d.Districts)
                    .FirstOrDefaultAsync(d => d.DioceseId == id);

                if (diocese == null)
                {
                    _logger.LogWarning("Diocese with ID {Id} not found.", id);
                }

                return diocese;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching diocese with ID {Id}.", id);
                throw;
            }
        }

        public async Task AddAsync(Diocese diocese)
        {
            _logger.LogInformation("Adding new diocese: {@Diocese}", diocese);
            try
            {
                await _context.Dioceses.AddAsync(diocese);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Diocese added successfully with ID: {Id}", diocese.DioceseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding diocese.");
                throw;
            }
        }

        public async Task UpdateAsync(Diocese diocese)
        {
            _logger.LogInformation("Updating diocese with ID: {Id}", diocese.DioceseId);
            try
            {
                var existingDiocese = await _context.Dioceses.FindAsync(diocese.DioceseId);
                if (existingDiocese == null)
                {
                    _logger.LogWarning("Diocese with ID {Id} not found.", diocese.DioceseId);
                    throw new KeyNotFoundException("Diocese not found.");
                }

                _context.Entry(existingDiocese).CurrentValues.SetValues(diocese);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Diocese with ID {Id} updated successfully.", diocese.DioceseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating diocese with ID {Id}.", diocese.DioceseId);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting diocese with ID: {Id}", id);
            try
            {
                var diocese = await _context.Dioceses.FindAsync(id);
                if (diocese == null)
                {
                    _logger.LogWarning("Diocese with ID {Id} not found.", id);
                    throw new KeyNotFoundException("Diocese not found.");
                }

                _context.Dioceses.Remove(diocese);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Diocese with ID {Id} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting diocese with ID {Id}.", id);
                throw;
            }
        }
    }
}

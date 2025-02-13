using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.Extensions.Logging;

namespace ChurchServices
{
    public class FamilyService : IFamilyService
    {
        private readonly IFamilyRepository _familyRepository;
        private readonly ILogger<FamilyService> _logger;

        public FamilyService(IFamilyRepository familyRepository, ILogger<FamilyService> logger)
        {
            _familyRepository = familyRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Family>> GetFamiliesAsync(int? parishId, int? unitId, int? familyId)
        {
            _logger.LogInformation("Fetching families with ParishId: {ParishId}, UnitId: {UnitId}, FamilyId: {FamilyId}", parishId, unitId, familyId);
            var families = await _familyRepository.GetFamiliesAsync(parishId, unitId, familyId);
            _logger.LogInformation("Fetched {Count} families", families?.ToString() ?? "0");
            return families;
        }

        public async Task<Family?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching family with Id: {Id}", id);
            var family = await _familyRepository.GetByIdAsync(id);
            if (family == null)
            {
                _logger.LogWarning("Family with Id: {Id} not found", id);
            }
            return family;
        }

        public async Task<IEnumerable<Family>> AddOrUpdateAsync(IEnumerable<Family> requests)
        {
            _logger.LogInformation("Processing {Count} families for AddOrUpdate.", requests?.ToString() ?? "0");
            var createdFamilies = new List<Family>();

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    _logger.LogInformation("Adding new family: {FamilyName}", request.FamilyName);
                    var createdFamily = await AddAsync(request);
                    createdFamilies.Add(createdFamily);
                }
                else if (request.Action == "UPDATE")
                {
                    _logger.LogInformation("Updating family with Id: {Id}", request.FamilyId);
                    var createdFamily = await UpdateAsync(request);
                    createdFamilies.Add(createdFamily);
                }
                else
                {
                    _logger.LogWarning("Invalid action '{Action}' specified for family Id: {Id}", request.Action, request.FamilyId);
                    throw new ArgumentException("Invalid action specified");
                }
            }

            _logger.LogInformation("Successfully processed {Count} families for AddOrUpdate.", createdFamilies.Count);
            return createdFamilies;
        }

        public async Task<Family> AddAsync(Family family)
        {
            _logger.LogInformation("Adding family: {FamilyName}", family.FamilyName);
            var addedFamily = await _familyRepository.AddAsync(family);
            _logger.LogInformation("Successfully added family Id: {Id}", addedFamily.FamilyId);
            return addedFamily;
        }

        public async Task<Family> UpdateAsync(Family family)
        {
            if (family.JoinDate.HasValue)
            {
                family.JoinDate = DateTime.SpecifyKind(family.JoinDate.Value, DateTimeKind.Utc);
            }
            _logger.LogInformation("Updating family with Id: {Id}", family.FamilyId);
            var updatedFamily = await _familyRepository.UpdateAsync(family);
            _logger.LogInformation("Successfully updated family Id: {Id}", updatedFamily.FamilyId);
            return updatedFamily;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting family with Id: {Id}", id);
            await _familyRepository.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted family with Id: {Id}", id);
        }
    }
}

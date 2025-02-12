using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchData.DTOs;
using ChurchRepositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog.Core;

namespace ChurchServices
{
    public class ParishService : IParishService
    {
        private readonly IParishRepository _parishRepository;
        private readonly ILogger<ParishService> _logger;
        private readonly IUnitRepository _unitRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly ITransactionHeadRepository _transactionHeadRepository;
        private readonly IBankRepository _bankRepository;
        private readonly IFinancialYearRepository _financialYearRepository;
        private readonly IConfiguration _configuration;


        //ILogger<ParishService> logger
        //_parishRepository = parishRepository ?? throw new ArgumentNullException(nameof(parishRepository));
        //_logger = logger ?? throw new ArgumentNullException(nameof(logger));
        public ParishService(
        IParishRepository parishRepository,
        IUnitRepository unitRepository,
        IFamilyRepository familyRepository,
        ITransactionHeadRepository transactionHeadRepository,
        IBankRepository bankRepository,
        IFinancialYearRepository financialYearRepository,
        ILogger<ParishService> logger,
        IConfiguration configuration)
        {
            _parishRepository = parishRepository ?? throw new ArgumentNullException(nameof(parishRepository));
            _unitRepository = unitRepository;
            _familyRepository = familyRepository;
            _transactionHeadRepository = transactionHeadRepository;
            _bankRepository = bankRepository;
            _financialYearRepository = financialYearRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<IEnumerable<Parish>> GetAllAsync()
        {
            try
            {
                _logger.LogInformation("Fetching all parishes.");
                var parishes = await _parishRepository.GetAllAsync();
                _logger.LogInformation("Retrieved {Count} parishes.", parishes?.ToString()?.Length ?? 0);
                return parishes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving all parishes.");
                throw;
            }
        }

        public async Task<Parish?> GetByIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Fetching parish with ID {ParishId}.", id);
                var parish = await _parishRepository.GetByIdAsync(id);
                if (parish == null)
                {
                    _logger.LogWarning("Parish with ID {ParishId} not found.", id);
                }
                return parish;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving parish with ID {ParishId}.", id);
                throw;
            }
        }

        public async Task<Parish> AddAsync(Parish parish)
        {
            try
            {
                if (parish == null)
                {
                    throw new ArgumentNullException(nameof(parish), "Parish object cannot be null.");
                }

                _logger.LogInformation("Adding new parish: {ParishName}.", parish.ParishName);

                var createdParish = await _parishRepository.AddAsync(parish);

                _logger.LogInformation("Parish added successfully with ID {ParishId}.", createdParish.ParishId);

                _logger.LogInformation("Adding dependent entities");

                await LoadDummyDataAsync(createdParish.ParishId);

                return createdParish;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding parish: {ParishName}.", parish?.ParishName);
                throw;
            }
        }

        public async Task UpdateAsync(Parish parish)
        {
            try
            {
                if (parish == null)
                {
                    throw new ArgumentNullException(nameof(parish), "Parish object cannot be null.");
                }

                _logger.LogInformation("Updating parish with ID {ParishId}.", parish.ParishId);
                await _parishRepository.UpdateAsync(parish);
                _logger.LogInformation("Parish with ID {ParishId} updated successfully.", parish.ParishId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating parish with ID {ParishId}.", parish?.ParishId);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                _logger.LogInformation("Deleting parish with ID {ParishId}.", id);
                await _parishRepository.DeleteAsync(id);
                _logger.LogInformation("Parish with ID {ParishId} deleted successfully.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting parish with ID {ParishId}.", id);
                throw;
            }
        }

        public async Task<ParishDetailsDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false)
        {
            try
            {
                _logger.LogInformation("Fetching parish details for ID {ParishId}. IncludeFamilyMembers: {IncludeFamilyMembers}", parishId, includeFamilyMembers);
                var parishDetails = await _parishRepository.GetParishDetailsAsync(parishId, includeFamilyMembers);

                if (parishDetails == null)
                {
                    _logger.LogWarning("Parish details not found for ID {ParishId}.", parishId);
                }

                return parishDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving parish details for ID {ParishId}.", parishId);
                throw;
            }
        }

        public async Task LoadDummyDataAsync(int parishId)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["DummyDataPath"]);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Dummy data file not found.");
            }

            if (File.Exists(filePath))
            {
                var jsonData = await File.ReadAllTextAsync(filePath);
                var parishData = JsonConvert.DeserializeObject<ParishDetailsDto>(jsonData);

                if (parishData != null)
                {
                    await InsertChildEntities(parishId, parishData);
                }
            }

        }
        // Helper method to insert child tables
        private async Task InsertChildEntities(int parishId, ParishDetailsDto parishData)
        {
            // Step 1: Insert Units
            var createdUnit= new Unit();
            foreach (var unit in parishData.Units)
            {
                var unitEntity = new Unit
                {
                    ParishId = parishId,
                    UnitName = unit.UnitName,
                    Description = unit.Description,
                    UnitPresident = unit.UnitPresident,
                    UnitSecretary = unit.UnitSecretary
                };
                
                createdUnit = await _unitRepository.AddAsync(unitEntity);
            }

            // Step 2: Insert Families
            foreach (var family in parishData.Families)
            {
                var familyEntity = new Family
                {
                    UnitId = createdUnit.UnitId,
                    ParishId = parishId,
                    FamilyName = family.FamilyName,
                    Category = family.Category,
                    FamilyNumber = family.FamilyNumber,
                    Status = family.Status,
                    HeadName = family.HeadName,
                    JoinDate = family.JoinDate
                };
                await _familyRepository.AddAsync(familyEntity);
            }

            // Step 3: Insert Transaction Heads
            foreach (var transactionHead in parishData.TransactionHeads)
            {
                var transactionHeadEntity = new TransactionHead
                {
                    ParishId = parishId,
                    HeadName = transactionHead.HeadName,
                    Type = transactionHead.Type,
                    Description = transactionHead.Description
                };
                await _transactionHeadRepository.AddAsync(transactionHeadEntity);
            }

            // Step 4: Insert Banks
            foreach (var bank in parishData.Banks)
            {
                var bankEntity = new Bank
                {
                    ParishId = parishId,
                    BankName = bank.BankName,
                    AccountNumber = bank.AccountNumber,
                    OpeningBalance = bank.OpeningBalance,
                    CurrentBalance = bank.CurrentBalance
                };
                await _bankRepository.AddAsync(bankEntity);
            }

            // Step 5: Insert Financial Years
            foreach (var financialYear in parishData.FinancialYears)
            {
                var financialYearEntity = new FinancialYear
                {
                    ParishId = parishId,
                    StartDate = financialYear.StartDate,
                    EndDate = financialYear.EndDate,
                    IsLocked = financialYear.IsLocked,
                    Description = financialYear.Description
                };
                await _financialYearRepository.AddAsync(financialYearEntity);
            }
        }
    }
}

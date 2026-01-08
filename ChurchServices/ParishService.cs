using AutoMapper;
using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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
        private readonly IMapper _mapper;

        public ParishService(
            IParishRepository parishRepository,
            IUnitRepository unitRepository,
            IFamilyRepository familyRepository,
            ITransactionHeadRepository transactionHeadRepository,
            IBankRepository bankRepository,
            IFinancialYearRepository financialYearRepository,
            ILogger<ParishService> logger,
            IConfiguration configuration,
            IMapper mapper)
        {
            _parishRepository = parishRepository ?? throw new ArgumentNullException(nameof(parishRepository));
            _unitRepository = unitRepository;
            _familyRepository = familyRepository;
            _transactionHeadRepository = transactionHeadRepository;
            _bankRepository = bankRepository;
            _financialYearRepository = financialYearRepository;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<IEnumerable<ParishDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all parishes.");
            var parishes = await _parishRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<ParishDto>>(parishes);
        }

        public async Task<ParishDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching parish with ID {ParishId}.", id);
            var parish = await _parishRepository.GetByIdAsync(id);
            return _mapper.Map<ParishDto?>(parish);
        }

        public async Task<ParishDto> AddAsync(ParishDto parishDto)
        {
            _logger.LogInformation("Adding new parish: {ParishName}.", parishDto.ParishName);

            if (parishDto.Latitude.HasValue && (parishDto.Latitude < -90 || parishDto.Latitude > 90))
                throw new ArgumentException("Latitude must be between -90 and 90");

            if (parishDto.Longitude.HasValue && (parishDto.Longitude < -180 || parishDto.Longitude > 180))
                throw new ArgumentException("Longitude must be between -180 and 180");

            var parish = _mapper.Map<Parish>(parishDto);
            var createdParish = await _parishRepository.AddAsync(parish);
          //  await LoadDummyDataAsync(createdParish.ParishId);
            return _mapper.Map<ParishDto>(createdParish);

        }

        public async Task<ParishDto> UpdateAsync(ParishDto parishDto)
        {
            _logger.LogInformation("Updating parish with ID {ParishId}.", parishDto.ParishId);

            if (parishDto.Latitude.HasValue && (parishDto.Latitude < -90 || parishDto.Latitude > 90))
                throw new ArgumentException("Latitude must be between -90 and 90");

            if (parishDto.Longitude.HasValue && (parishDto.Longitude < -180 || parishDto.Longitude > 180))
                throw new ArgumentException("Longitude must be between -180 and 180");


            var parish = _mapper.Map<Parish>(parishDto);
            await _parishRepository.UpdateAsync(parish);
            return parishDto;
        }

        public async Task DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting parish with ID {ParishId}.", id);
            await _parishRepository.DeleteAsync(id);
        }

        public async Task<ParishDetailsBasicDto> GetParishDetailsAsync(int parishId, bool includeFamilyMembers = false)
        {
            _logger.LogInformation("Fetching parish details for ID {ParishId}.", parishId);
            var parishDetails = await _parishRepository.GetParishDetailsAsync(parishId, includeFamilyMembers);
            return parishDetails;
        }

        private async Task LoadDummyDataAsync(int parishId)
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _configuration["DummyDataPath"]);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Dummy data file not found.");
            }

            var jsonData = await File.ReadAllTextAsync(filePath);
            var parishData = JsonConvert.DeserializeObject<ParishDetailsDto>(jsonData);
            if (parishData != null)
            {
                await InsertChildEntities(parishId, parishData);
            }
        }

        private async Task InsertChildEntities(int parishId, ParishDetailsDto parishData)
        {
            _logger.LogInformation("Inserting child entities for parish ID {ParishId}.", parishId);
            var unitId = 0;
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
                await _unitRepository.AddAsync(unitEntity);
                unitId = unitEntity.UnitId;
            }

            foreach (var family in parishData.Families)
            {
                var familyEntity = new Family
                {
                    ParishId = parishId,
                    UnitId = unitId,
                    FamilyName = family.FamilyName,
                    Category = family.Category,
                    FamilyNumber = family.FamilyNumber,
                    Status = family.Status,
                    HeadName = family.HeadName,
                    JoinDate = family.JoinDate
                };
                await _familyRepository.AddAsync(familyEntity);
            }

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

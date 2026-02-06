using ChurchContracts.Interfaces.Services;
using ChurchContracts.Interfaces.Repositories;
using ChurchDTOs.DTOs.Entities;

namespace ChurchServices
{
    public class PublicService : IPublicService 
    {
        private readonly IPublicRepository _publicRepository;

        public PublicService(IPublicRepository publicRepository)
        {
            _publicRepository = publicRepository;
        }

        public async Task<ParishesAllDto> GetAllParishesAsync()
        {
            var parishes = await _publicRepository.GetAllParishesAsync();
            return parishes;
        }

        public async Task<IEnumerable<TransactionHeadBasicDto>> GetTransactionHeadsByParishIdAsync(int parishId)
        {
            return await _publicRepository.GetTransactionHeadsByParishIdAsync(parishId);
        }
    }
}

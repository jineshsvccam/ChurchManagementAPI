using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyMemberService
    {
        Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int? parishId, int? familyId, int? memberId);
        Task<FamilyMember?> GetByIdAsync(int id);
        Task<IEnumerable<FamilyMember>> AddOrUpdateAsync(IEnumerable<FamilyMember> requests);
        Task<FamilyMember> UpdateAsync(FamilyMember familyMember);
        Task DeleteAsync(int id);
        Task<FamilyMember> AddAsync(FamilyMember familyMember);
    }
}

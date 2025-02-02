using ChurchData;

namespace ChurchContracts
{
    public interface IFamilyMemberRepository
    {
        Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int? parishId, int? familyId, int? memberId);
        Task<FamilyMember?> GetByIdAsync(int id);
        Task<FamilyMember> AddAsync(FamilyMember familyMember);
        Task<FamilyMember> UpdateAsync(FamilyMember familyMember);
        Task DeleteAsync(int id);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;

namespace ChurchServices
{
    public class FamilyMemberService : IFamilyMemberService
    {
        private readonly IFamilyMemberRepository _familyMemberRepository;

        public FamilyMemberService(IFamilyMemberRepository familyMemberRepository)
        {
            _familyMemberRepository = familyMemberRepository;
        }

        public async Task<IEnumerable<FamilyMember>> GetFamilyMembersAsync(int? parishId, int? familyId, int? memberId)
        {
            return await _familyMemberRepository.GetFamilyMembersAsync(parishId, familyId, memberId);
        }

        public async Task<FamilyMember?> GetByIdAsync(int id)
        {
            return await _familyMemberRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<FamilyMember>> AddOrUpdateAsync(IEnumerable<FamilyMember> requests)
        {
            var createdFamilyMembers = new List<FamilyMember>();

            foreach (var request in requests)
            {
                if (request.Action == "INSERT")
                {
                    var createdFamilyMember = await AddAsync(request);
                    createdFamilyMembers.Add(createdFamilyMember);
                }
                else if (request.Action == "UPDATE")
                {
                    var updatedFamilyMember = await UpdateAsync(request);
                    createdFamilyMembers.Add(updatedFamilyMember);
                }
                else
                {
                    throw new ArgumentException("Invalid action specified");
                }
            }
            return createdFamilyMembers;
        }

        public async Task<FamilyMember> AddAsync(FamilyMember familyMember)
        {
            var addedFamilyMember = await _familyMemberRepository.AddAsync(familyMember);
            return addedFamilyMember;
        }

        public async Task<FamilyMember> UpdateAsync(FamilyMember familyMember)
        {
            var updatedFamilyMember = await _familyMemberRepository.UpdateAsync(familyMember);
            return updatedFamilyMember;
        }

        public async Task DeleteAsync(int id)
        {
            await _familyMemberRepository.DeleteAsync(id);
        }
    }
}

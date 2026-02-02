using AutoMapper;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchRepositories.Admin
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public RoleRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Role> GetRoleByIdAsync(Guid roleId)
        {
            return await _context.Roles.FindAsync(roleId);
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            // Exclude the built-in Admin role from the returned list
            var roles = await _context.Roles
                .Where(r => r.NormalizedName != "ADMIN")
                .ToListAsync();
            return _mapper.Map<IEnumerable<RoleDto>>(roles);
        }

        public async Task<RoleDto> AddRoleAsync(RoleDto role)
        {
            var normalizedRoleName = role.Name.ToUpper();  // Ensure consistency
            var existingRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == normalizedRoleName);
            if (existingRole != null)
            {
                throw new Exception("Role already exists.");
            }
            var roleo = _mapper.Map<Role>(role);
            roleo.Id = new Guid();
            roleo.NormalizedName = role.Name.ToUpper();
            roleo.ConcurrencyStamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssffff");
            await _context.Roles.AddAsync(roleo);
            await _context.SaveChangesAsync();
            return _mapper.Map<RoleDto>(roleo);
        }

        public async Task<RoleDto> UpdateRoleAsync(RoleDto role)
        {
            var existingRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == role.Id);
            if (existingRole == null)
            {
                throw new Exception("Role does not exist.");
            }

            // Check if another role with the same name exists
            var duplicateRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.NormalizedName == role.Name.ToUpper());

            if (duplicateRole != null)
            {
                throw new Exception("A role with this name already exists.");
            }

            // Update existing role
            _mapper.Map(role, existingRole);
            existingRole.NormalizedName = role.Name.ToUpper();
            existingRole.ConcurrencyStamp = Guid.NewGuid().ToString(); // Ensure concurrency control

            await _context.SaveChangesAsync();
            return _mapper.Map<RoleDto>(existingRole);
        }


        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _context.Roles.FindAsync(roleId);
            
            if (role == null)
            {
                throw new Exception("Role does not exists.");
            }

            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }
    }
}

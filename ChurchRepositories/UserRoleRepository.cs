using ChurchContracts;
using ChurchData;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<UserRole> GetUserRoleByIdAsync(int userId, int roleId)
        {
            var identityUserRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);

            if (identityUserRole == null)
            {
                return null;
            }

            var user = await _context.Users.FindAsync(identityUserRole.UserId);
            var role = await _context.Roles.FindAsync(identityUserRole.RoleId);

            return new UserRole
            {
                UserId = identityUserRole.UserId,
                RoleId = identityUserRole.RoleId,
                User = user,
                Role = role
            };
        }



        public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => userRoles.Select(ur => ur.RoleId).Contains(r.Id))
                .ToListAsync();

            // Use async lambda expressions here
            var result = await Task.WhenAll(userRoles.Select(async ur => new UserRole
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                User = await _context.Users.FindAsync(ur.UserId), // Load User entity
                Role = roles.FirstOrDefault(r => r.Id == ur.RoleId) // Load Role entity
            }));

            return result;
        }



        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task DeleteUserRoleAsync(int userId, int roleId)
        {
            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }
    }
}

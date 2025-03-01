using AutoMapper;
using ChurchCommon.Utils;
using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChurchRepositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserRoleRepository(ApplicationDbContext context,IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<UserRole> GetUserRoleByIdAsync(Guid userId, Guid roleId)
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
                Status = identityUserRole.Status,
                ApprovedBy = identityUserRole.ApprovedBy,
                RequestedAt = identityUserRole.RequestedAt,
                ApprovedAt = identityUserRole.ApprovedAt,
                User = user,
                Role = role
            };
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesByUserIdAsync(Guid userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .ToListAsync();

            var roles = await _context.Roles
                .Where(r => userRoles.Select(ur => ur.RoleId).Contains(r.Id))
                .ToListAsync();

            var result = await Task.WhenAll(userRoles.Select(async ur => new UserRole
            {
                UserId = ur.UserId,
                RoleId = ur.RoleId,
                Status = ur.Status,
                ApprovedBy = ur.ApprovedBy,
                RequestedAt = ur.RequestedAt,
                ApprovedAt = ur.ApprovedAt,
                User = await _context.Users.FindAsync(ur.UserId),
                Role = roles.FirstOrDefault(r => r.Id == ur.RoleId)
            }));

            return result;
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task DeleteUserRoleAsync(Guid userId, Guid roleId)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<PendingUserRoleDto>> GetPendingUserRolesAsync()
        {
            Guid currentUserId = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

            // Get the logged-in user's role and parish
            var currentUserRole = await _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Include(ur => ur.User.Parish)
                .Include(ur => ur.User.Family)
                .FirstOrDefaultAsync(ur => ur.UserId == currentUserId && ur.Status == RoleStatus.Approved);

            if (currentUserRole == null)
            {
                throw new Exception("User role not found.");
            }

            var currentRoleName = currentUserRole.Role.Name;
            int? parishId = currentRoleName == "Admin" ? null : currentUserRole.User?.ParishId;

            // Define roles to filter based on current user's role
            var rolesToShow = currentRoleName switch
            {
                "Admin" => new[] { "Secretary", "Trustee", "Priest" },
                "Secretary" or "Trustee" or "Priest" => new[] { "FamilyMember" },
                _ => Array.Empty<string>()
            };

            if (rolesToShow.Length == 0)
            {
                return Enumerable.Empty<PendingUserRoleDto>(); // No roles to display
            }

            // Start with base query
            var pendingRolesQuery = _context.UserRoles.AsQueryable();

            // Apply filtering conditions
            pendingRolesQuery = pendingRolesQuery
                .Where(ur => ur.Status == RoleStatus.Pending &&
                             rolesToShow.Contains(ur.Role.Name));

            // Apply parish filtering only if the user is NOT an Admin
            if (parishId.HasValue)
            {
                pendingRolesQuery = pendingRolesQuery.Where(ur => ur.User.ParishId == parishId);
            }

            // Apply Includes at the end
            var pendingRoles = await pendingRolesQuery
                .Include(ur => ur.Role)  // Include Role for RoleName
                .Include(ur => ur.User)  // Include User for FullName, PhoneNumber, Email
                .ToListAsync();

            return _mapper.Map<IEnumerable<PendingUserRoleDto>>(pendingRoles);
        }

        public async Task<UserRole?> ApproveUserRoleAsync(ApproveRoleDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            Guid currentUserId = UserHelper.GetCurrentUserIdGuid(_httpContextAccessor);

            try
            {
                var userRole = await _context.UserRoles
                    .Include(ur => ur.User) // Include User to update status
                    .FirstOrDefaultAsync(ur => ur.UserId == dto.UserId && ur.RoleId == dto.RoleId);

                if (userRole == null)
                {
                    return null;
                }

                if (!Enum.TryParse(dto.Status, true, out RoleStatus newStatus))
                {
                    throw new ArgumentException("Invalid role status provided.");
                }

                userRole.Status = newStatus;
                userRole.ApprovedAt = DateTime.UtcNow;
                userRole.ApprovedBy = currentUserId;

                //  If approved, ensure user is active
                if (newStatus == RoleStatus.Approved && userRole.User.Status != UserStatus.Active)
                {
                    userRole.User.Status = UserStatus.Active;
                    userRole.User.UpdatedAt = DateTime.UtcNow;
                }

                _context.UserRoles.Update(userRole);
                _context.Users.Update(userRole.User); //  Ensure user status is updated
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return userRole;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
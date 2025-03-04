using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ChurchData;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ChurchCommon.Utils
{
    public static class UserHelper
    {
        public static int GetCurrentUserId(IHttpContextAccessor httpContextAccessor)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);

            return 1;
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                throw new UnauthorizedAccessException("User ID not found.");
            }

            return userId;
        }

        public static Guid GetCurrentUserIdGuid(IHttpContextAccessor httpContextAccessor)
        {
            var userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                throw new UnauthorizedAccessException("User ID not found.");
            }
            return userId;
        }

        public static async Task<(string RoleName, int? ParishId, int? FamilyId)> GetCurrentUserRoleAsync(
         IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, ILogger logger)
        {
            Guid currentUserId = GetCurrentUserIdGuid(httpContextAccessor);

            var currentUserRole = await context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.User)
                .Include(ur => ur.User.Parish)
                .Include(ur => ur.User.Family)
                .FirstOrDefaultAsync(ur => ur.UserId == currentUserId);

            if (currentUserRole == null)
            {
                logger.LogError("User role not found for user with id {currentUserId}", currentUserId);
                throw new InvalidOperationException("User role not found.");
            }

            return (currentUserRole.Role.Name, currentUserRole.User?.ParishId, currentUserRole.User?.FamilyId);
        }
    }
}

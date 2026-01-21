using ChurchCommon.Utils;
using ChurchData;
using ChurchDTOs.DTOs.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using System.Collections;
using System.Threading.Tasks;

namespace ChurchManagementAPI.Controllers.Base
{
    [Authorize(Policy = "ManagementPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ManagementAuthorizedController<T> : ControllerBase, IAsyncActionFilter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<T> _logger;

        protected ManagementAuthorizedController(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<T> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _logger = logger;
        }

        protected async Task<int?> GetCurrentParishIdAsync()
        {
            var (_, userParishId, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);
            return userParishId;
        }

        protected async Task<bool> IsAdminUserAsync()
        {
            var (roleName, _, _) = await UserHelper.GetCurrentUserRoleAsync(_httpContextAccessor, _context, _logger);
            return string.Equals(roleName, "Admin", StringComparison.OrdinalIgnoreCase);
        }

        [NonAction]
        public async Task ValidateParishIdAsync(int parishId)
        {
            // Admin users can access any parish
            if (await IsAdminUserAsync())
            {
                return;
            }

            var currentParishId = await GetCurrentParishIdAsync();
            if (currentParishId == null || currentParishId != parishId)
            {
                _logger.LogWarning($"Unauthorized access attempt. Expected Parish ID: {currentParishId}, Received: {parishId}");
                throw new UnauthorizedAccessException("You are not authorized to access data from another parish.");
            }
        }

        protected async Task ValidateParishEntityAsync(object entityObj)
        {
            // Admin users can access any parish data
            if (await IsAdminUserAsync())
            {
                return;
            }

            var currentParishId = await GetCurrentParishIdAsync();
            if (entityObj is IParishEntity singleEntity)
            {
                if (singleEntity.ParishId != currentParishId)
                {
                    _logger.LogWarning($"Unauthorized access for returned entity. Expected Parish ID: {currentParishId}, Entity Parish ID: {singleEntity.ParishId}");
                    throw new UnauthorizedAccessException("You are not authorized to access this resource.");
                }
            }
            else if (entityObj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is IParishEntity parishEntity)
                    {
                        if (parishEntity.ParishId != currentParishId)
                        {
                            _logger.LogWarning($"Unauthorized access for an item in collection. Expected Parish ID: {currentParishId}, Item Parish ID: {parishEntity.ParishId}");
                            throw new UnauthorizedAccessException("You are not authorized to access one or more items in this resource.");
                        }
                    }
                }
            }
        }

        protected async Task ValidateInputBodyAsync(object bodyObj)
        {
            // Admin users can modify any parish data
            if (await IsAdminUserAsync())
            {
                return;
            }

            var currentParishId = await GetCurrentParishIdAsync();
            if (bodyObj is IParishEntity singleEntity)
            {
                if (singleEntity.ParishId != currentParishId)
                {
                    _logger.LogWarning($"Unauthorized input body. Expected Parish ID: {currentParishId}, Entity Parish ID: {singleEntity.ParishId}");
                    throw new UnauthorizedAccessException("You are not authorized to modify data from another parish.");
                }
            }
            else if (bodyObj is IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    if (item is IParishEntity parishEntity)
                    {
                        if (parishEntity.ParishId != currentParishId)
                        {
                            _logger.LogWarning($"Unauthorized input body item. Expected Parish ID: {currentParishId}, Item Parish ID: {parishEntity.ParishId}");
                            throw new UnauthorizedAccessException("You are not authorized to modify one or more items from another parish.");
                        }
                    }
                }
            }
        }

        [NonAction]
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Validate parishId query parameter
            if (context.ActionArguments.TryGetValue("parishId", out var parishIdObj) && parishIdObj is int parishId)
            {
                try
                {
                    await ValidateParishIdAsync(parishId);
                }
                catch (UnauthorizedAccessException ex)
                {
                    context.Result = new ForbidResult();
                    _logger.LogWarning($"Access denied for input parish {parishId}: {ex.Message}");
                    return;
                }
            }

            // Validate input body DTOs
            foreach (var argument in context.ActionArguments.Values)
            {
                if (argument == null)
                {
                    continue;
                }

                // Check if it's an IParishEntity or a collection of IParishEntity
                if (argument is IParishEntity || (argument is IEnumerable enumerable && enumerable.Cast<object>().FirstOrDefault() is IParishEntity))
                {
                    try
                    {
                        await ValidateInputBodyAsync(argument);
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        context.Result = new ForbidResult();
                        _logger.LogWarning($"Access denied for input body: {ex.Message}");
                        return;
                    }
                }
            }

            var executedContext = await next();

            if (executedContext.Result is ObjectResult objectResult && objectResult.Value != null)
            {
                try
                {
                    await ValidateParishEntityAsync(objectResult.Value);
                }
                catch (UnauthorizedAccessException ex)
                {
                    executedContext.Result = new ForbidResult();
                    _logger.LogWarning($"Access denied for output: {ex.Message}");
                    return;
                }
            }
        }
    }
}

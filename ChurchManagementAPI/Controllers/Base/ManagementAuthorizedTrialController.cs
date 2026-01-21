using ChurchData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Base
{
    [Authorize(Policy = "ManagementPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public class ManagementAuthorizedTrialController : ManagementAuthorizedController<ManagementAuthorizedTrialController>
    {
        public ManagementAuthorizedTrialController(
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            // Shared functionality for controllers requiring Admin, Secretary, and Trustee roles.
        }
    }
}

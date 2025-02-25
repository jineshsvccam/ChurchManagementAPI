using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Base
{
    [Authorize(Policy = "ManagementPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class ManagementAuthorizedController : ControllerBase
    {
        // Shared functionality for controllers requiring Admin, Secretary, and Trustee roles.
    }
}

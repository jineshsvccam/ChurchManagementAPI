using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Base
{
    [Authorize(Policy = "AdminPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class AdminAuthorizedController : ControllerBase
    {
        // Shared functionality for Admin-only controllers can be added here.
    }
}

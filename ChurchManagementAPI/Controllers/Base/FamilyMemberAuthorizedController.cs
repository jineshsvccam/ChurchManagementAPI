using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers.Base
{
    [Authorize(Policy = "FamilyMemberPolicy")]
    [ApiController]
    [Route("api/[controller]")]
    public abstract class FamilyMemberAuthorizedController : ControllerBase
    {
        // Shared functionality for controllers accessible to Admin, Secretary, Trustee, and FamilyMember.
    }
}

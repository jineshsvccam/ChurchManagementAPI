using ChurchContracts;
using ChurchContracts.Interfaces.Services;
using ChurchDTOs.DTOs.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PublicController : ControllerBase
    {
        private readonly IPublicService _publicService;
        private readonly IRoleService _roleService;
        public PublicController(IPublicService publicService,IRoleService roleService)
        {
            _publicService = publicService;
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<ActionResult<ParishesAllDto>> GetAll()
        {
            var parishesAll = await _publicService.GetAllParishesAsync();
            return Ok(parishesAll);
        }

      
    }
}

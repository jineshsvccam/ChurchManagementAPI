using ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
   // [ApiExplorerSettings(IgnoreApi = false)]
    public class FinancialYearController : ManagementAuthorizedTrialController
    {
        private readonly IFinancialYearService _financialYearService;

        public FinancialYearController(IFinancialYearService financialYearService,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context,
            ILogger<FinancialYearController> logger)
            //: base(httpContextAccessor, context, logger)
        {
            _financialYearService = financialYearService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFinancialYears([FromQuery] int? parishId)
        {
            var financialYears = await _financialYearService.GetAllAsync(parishId);
            return Ok(financialYears);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetFinancialYearById(int id)
        {
            var financialYear = await _financialYearService.GetByIdAsync(id);
            if (financialYear == null)
            {
                return NotFound();
            }
            return Ok(financialYear);
        }

        [HttpPost]
        public async Task<IActionResult> AddFinancialYear([FromBody] FinancialYearDto financialYearDto)
        {
            var createdFinancialYear = await _financialYearService.AddAsync(financialYearDto);
            return CreatedAtAction(nameof(GetFinancialYearById), new { id = createdFinancialYear.FinancialYearId }, createdFinancialYear);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFinancialYear(int id, [FromBody] FinancialYearDto financialYearDto)
        {
            if (id != financialYearDto.FinancialYearId)
            {
                return BadRequest("Financial Year ID mismatch.");
            }

            var updatedFinancialYear = await _financialYearService.UpdateAsync(financialYearDto);
            return Ok(updatedFinancialYear);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFinancialYear(int id)
        {
            await _financialYearService.DeleteAsync(id);
            return NoContent();
        }

        [HttpGet("parish/{parishId}/date/{date}")]
        public async Task<IActionResult> GetFinancialYearByDate(int parishId, DateTime date)
        {
            var financialYear = await _financialYearService.GetFinancialYearByDateAsync(parishId, date);
            if (financialYear == null)
            {
                return NotFound();
            }
            return Ok(financialYear);
        }


        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockFinancialYear(int id)
        {
            await _financialYearService.LockFinancialYearAsync(id);
            return Ok();
        }
    }
}

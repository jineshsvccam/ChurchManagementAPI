using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChurchContracts;
using ChurchData;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FinancialYearController : ControllerBase
    {
        private readonly IFinancialYearService _financialYearService;

        public FinancialYearController(IFinancialYearService financialYearService)
        {
            _financialYearService = financialYearService;
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

        [HttpGet]
        public async Task<IActionResult> GetAllFinancialYears()
        {
            var financialYears = await _financialYearService.GetAllAsync();
            return Ok(financialYears);
        }

        [HttpPost]
        public async Task<IActionResult> AddFinancialYear([FromBody] FinancialYear financialYear)
        {
            var createdFinancialYear = await _financialYearService.AddAsync(financialYear);
            return CreatedAtAction(nameof(GetFinancialYearById), new { id = createdFinancialYear.FinancialYearId }, createdFinancialYear);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFinancialYear(int id, [FromBody] FinancialYear financialYear)
        {
            if (id != financialYear.FinancialYearId)
            {
                return BadRequest("Financial Year ID mismatch.");
            }

            var updatedFinancialYear = await _financialYearService.UpdateAsync(financialYear);
            return Ok(updatedFinancialYear);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFinancialYear(int id)
        {
            await _financialYearService.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost("{id}/lock")]
        public async Task<IActionResult> LockFinancialYear(int id)
        {
            await _financialYearService.LockFinancialYearAsync(id);
            return Ok();
        }
    }
}

using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Payments
{
    public class MemberPaymentsController : ManagementAuthorizedTrialController
    {
        private readonly IMemberPaymentService _service;

        public MemberPaymentsController(IMemberPaymentService service,
                                      IHttpContextAccessor httpContextAccessor,
                                      ApplicationDbContext context,
                                      ILogger<ManagementAuthorizedTrialController> logger)
            : base(httpContextAccessor, context, logger)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberPaymentDto>>> GetByParishId([FromQuery] int parishId)
        {
            var payments = await _service.GetByParishIdAsync(parishId);
            return Ok(payments);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MemberPaymentDto>> GetById(int id)
        {
            var payment = await _service.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MemberPaymentCreateDto dto)
        {
            try
            {
                var created = await _service.AddAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.PaymentId }, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MemberPaymentUpdateDto dto)
        {
            if (id != dto.PaymentId)
            {
                return BadRequest("PaymentId mismatch");
            }

            var updated = await _service.UpdateAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

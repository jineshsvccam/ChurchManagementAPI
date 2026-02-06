using ChurchContracts;
using ChurchContracts.ChurchContracts;
using ChurchData;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChurchManagementAPI.Controllers.Payments
{
    public class MemberPaymentsController : FamilyMemberAuthorizedController
    {
        private readonly IMemberPaymentService _service;
        private readonly ILogger<MemberPaymentsController> _logger;

        public MemberPaymentsController(IMemberPaymentService service,
                                      ILogger<MemberPaymentsController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberPaymentDto>>> GetByParishId([FromQuery] int parishId)
        {
            var payments = await _service.GetByParishIdAsync(parishId);
            return Ok(payments);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MemberPaymentDto>> GetById(Guid id)
        {
            var payment = await _service.GetByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            return Ok(payment);
        }

        [HttpGet("receipt/{receiptId}")]
        public async Task<ActionResult<IEnumerable<MemberPaymentDto>>> GetByReceiptId(string receiptId, [FromQuery] int parishId)
        {
            var payments = await _service.GetByReceiptIdAsync(receiptId, parishId);
            return Ok(payments);
        }

        [Authorize(Policy = "ManagementPolicy")]
        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<MemberPaymentDto>>> GetPending([FromQuery] int parishId)
        {
            var payments = await _service.GetPendingByParishIdAsync(parishId);
            return Ok(payments);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] IEnumerable<MemberPaymentCreateDto> dtos)
        {
            try
            {
                var created = await _service.AddAsync(dtos);
                return CreatedAtAction(nameof(GetByParishId), created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MemberPaymentUpdateDto dto)
        {
            if (id != dto.PaymentId)
            {
                return BadRequest("PaymentId mismatch");
            }

            var updated = await _service.UpdateAsync(dto);
            return Ok(updated);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        [Authorize(Policy = "ManagementPolicy")]
        [HttpPost("approve-or-reject")]
        public async Task<IActionResult> ApproveOrReject([FromBody] MemberPaymentApprovalDto dto)
        {
            var results = await _service.ApproveOrRejectAsync(dto);
            return Ok(results);
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<MemberPaymentBulkItemDto> requests)
        {
            try
            {
                var results = await _service.AddOrUpdateAsync(requests);
                if (results.Any())
                {
                    return CreatedAtAction(nameof(GetByParishId), results);
                }
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}

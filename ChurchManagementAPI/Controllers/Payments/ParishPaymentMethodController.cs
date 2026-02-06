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
    public class ParishPaymentMethodController : FamilyMemberAuthorizedController
    {
        private readonly IParishPaymentMethodService _service;
        private readonly ILogger<ParishPaymentMethodController> _logger;

        public ParishPaymentMethodController(IParishPaymentMethodService service,
                                           ILogger<ParishPaymentMethodController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ParishPaymentMethodDto>>> GetByParishId([FromQuery] int parishId)
        {
            var methods = await _service.GetByParishIdAsync(parishId);
            return Ok(methods);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ParishPaymentMethodDto>> GetById(int id)
        {
            var method = await _service.GetByIdAsync(id);
            if (method == null)
            {
                return NotFound();
            }
            return Ok(method);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ParishPaymentMethodDto dto)
        {
            var created = await _service.AddAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = created.PaymentMethodId }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ParishPaymentMethodDto dto)
        {
            if (id != dto.PaymentMethodId)
            {
                return BadRequest();
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

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<ParishPaymentMethodDto> requests)
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

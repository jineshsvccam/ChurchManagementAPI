using ChurchContracts;
using ChurchDTOs.DTOs.Entities;
using ChurchManagementAPI.Controllers.Base;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementAPI.Controllers
{
    public class TransactionHeadController : ManagementAuthorizedController
    {
        private readonly ITransactionHeadService _transactionHeadService;
        private readonly ILogger<TransactionHeadController> _logger;

        public TransactionHeadController(ITransactionHeadService transactionHeadService, ILogger<TransactionHeadController> logger)
        {
            _transactionHeadService = transactionHeadService ?? throw new ArgumentNullException(nameof(transactionHeadService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransactionHeadDto>>> GetTransactionHeads([FromQuery] int? parishId, [FromQuery] int? headId)
        {
            _logger.LogInformation("Fetching transaction heads for ParishId: {ParishId}, HeadId: {HeadId}", parishId, headId);
            var transactionHeads = await _transactionHeadService.GetTransactionHeadsAsync(parishId, headId);
            return Ok(transactionHeads); // Return DTOs directly, assuming the service already maps them
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TransactionHeadDto>> GetById(int id)
        {
            _logger.LogInformation("Fetching transaction head by Id: {Id}", id);
            var transactionHead = await _transactionHeadService.GetByIdAsync(id);
            if (transactionHead == null)
            {
                _logger.LogWarning("Transaction head not found with Id: {Id}", id);
                return NotFound();
            }
            return Ok(transactionHead); // Return DTO directly
        }

        [HttpPost]
        public async Task<ActionResult<TransactionHeadDto>> Create([FromBody] TransactionHeadDto transactionHeadDto)
        {
            var createdHead = await _transactionHeadService.AddAsync(transactionHeadDto); // Assuming service method accepts and returns DTO
            return CreatedAtAction(nameof(GetById), new { id = createdHead.HeadId }, createdHead); // Return DTO directly
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] TransactionHeadDto transactionHeadDto)
        {
            if (id != transactionHeadDto.HeadId)
            {
                _logger.LogWarning("Mismatch between URL Id: {Id} and body Id: {BodyId}", id, transactionHeadDto.HeadId);
                return BadRequest("ID mismatch");
            }

            _logger.LogInformation("Updating transaction head Id: {Id}", id);
            var updatedHead = await _transactionHeadService.UpdateAsync(transactionHeadDto); // Assuming service method accepts DTO
            if (updatedHead == null)
            {
                _logger.LogWarning("Transaction head not found for update with Id: {Id}", id);
                return NotFound();
            }

            _logger.LogInformation("Successfully updated transaction head Id: {Id}", id);
            return Ok(updatedHead); // Return DTO directly
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Deleting transaction head Id: {Id}", id);
            await _transactionHeadService.DeleteAsync(id);
            _logger.LogInformation("Successfully deleted transaction head Id: {Id}", id);
            return NoContent();
        }

        [HttpPost("create-or-update")]
        public async Task<IActionResult> CreateOrUpdate([FromBody] IEnumerable<TransactionHeadDto> transactionHeadDtos)
        {
            _logger.LogInformation("Creating or updating transaction heads.");
            var createdTransactionHeads = await _transactionHeadService.AddOrUpdateAsync(transactionHeadDtos); // Assuming service method accepts DTOs
            if (createdTransactionHeads.Any())
            {
                _logger.LogInformation("Successfully created or updated transaction heads.");
                return CreatedAtAction(nameof(GetTransactionHeads), createdTransactionHeads); // Return DTOs directly
            }
            return Ok();
        }
    }
}

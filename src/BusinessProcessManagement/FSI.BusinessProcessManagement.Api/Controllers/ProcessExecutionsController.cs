using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSI.BusinessProcessManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProcessExecutionsController : ControllerBase
    {
        private readonly IProcessExecutionAppService _service;
        public ProcessExecutionsController(IProcessExecutionAppService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProcessExecutionDto>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:long}")]
        public async Task<ActionResult<ProcessExecutionDto>> GetById(long id)
        {
            var dto = await _service.GetByIdAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] ProcessExecutionDto dto)
        {
            var id = await _service.InsertAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] ProcessExecutionDto dto)
        {
            dto.ExecutionId = id;
            await _service.UpdateAsync(dto);
            return NoContent();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}

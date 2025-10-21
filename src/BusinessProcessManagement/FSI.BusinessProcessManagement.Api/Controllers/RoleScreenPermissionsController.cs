using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FSI.BusinessProcessManagement.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoleScreenPermissionsController : ControllerBase
    {
        private readonly IRoleScreenPermissionAppService _service;
        public RoleScreenPermissionsController(IRoleScreenPermissionAppService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoleScreenPermissionDto>>> GetAll()
            => Ok(await _service.GetAllAsync());

        [HttpGet("{id:long}")]
        public async Task<ActionResult<RoleScreenPermissionDto>> GetById(long id)
        {
            var dto = await _service.GetByIdAsync(id);
            return dto is null ? NotFound() : Ok(dto);
        }

        [HttpPost]
        public async Task<ActionResult<long>> Create([FromBody] RoleScreenPermissionDto dto)
        {
            var id = await _service.InsertAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        [HttpPut("{id:long}")]
        public async Task<IActionResult> Update(long id, [FromBody] RoleScreenPermissionDto dto)
        {
            dto.RoleScreenPermissionId = id;
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

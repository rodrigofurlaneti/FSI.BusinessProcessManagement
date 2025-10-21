using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public class RoleScreenPermissionAppService : IRoleScreenPermissionAppService
    {
        private readonly IRepository<RoleScreenPermission> _repo;
        private readonly IUnitOfWork _uow;

        public RoleScreenPermissionAppService(IRepository<RoleScreenPermission> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<RoleScreenPermissionDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(RoleScreenPermissionMapper.ToDto);

        public async Task<RoleScreenPermissionDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : RoleScreenPermissionMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(RoleScreenPermissionDto dto)
        {
            var e = RoleScreenPermissionMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(RoleScreenPermissionDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.RoleScreenPermissionId)
                    ?? throw new KeyNotFoundException("Permission not found.");
            RoleScreenPermissionMapper.CopyToExisting(e, dto);
            await _repo.UpdateAsync(e);
            await _uow.CommitAsync();
        }

        public async Task DeleteAsync(long id)
        {
            await _repo.DeleteAsync(id);
            await _uow.CommitAsync();
        }
    }
}

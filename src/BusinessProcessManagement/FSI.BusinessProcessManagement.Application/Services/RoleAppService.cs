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
    public class RoleAppService : IRoleAppService
    {
        private readonly IRepository<Role> _repo;
        private readonly IUnitOfWork _uow;

        public RoleAppService(IRepository<Role> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<RoleDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(RoleMapper.ToDto);

        public async Task<RoleDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : RoleMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(RoleDto dto)
        {
            var e = RoleMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(RoleDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.RoleId) ?? throw new KeyNotFoundException("Role not found.");
            RoleMapper.CopyToExisting(e, dto);
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

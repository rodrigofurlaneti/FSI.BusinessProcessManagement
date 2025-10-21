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
    public class UserRoleAppService : IUserRoleAppService
    {
        private readonly IRepository<UserRole> _repo;
        private readonly IUnitOfWork _uow;

        public UserRoleAppService(IRepository<UserRole> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<UserRoleDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(UserRoleMapper.ToDto);

        public async Task<UserRoleDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : UserRoleMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(UserRoleDto dto)
        {
            var e = UserRoleMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(UserRoleDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.UserRoleId) ?? throw new KeyNotFoundException("UserRole not found.");
            UserRoleMapper.CopyToExisting(e, dto);
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

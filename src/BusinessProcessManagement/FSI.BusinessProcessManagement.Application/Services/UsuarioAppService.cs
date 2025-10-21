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
    public class UsuarioAppService : IUsuarioAppService
    {
        private readonly IRepository<User> _repo;
        private readonly IUnitOfWork _uow;

        public UsuarioAppService(IRepository<User> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<UsuarioDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(UsuarioMapper.ToDto);

        public async Task<UsuarioDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : UsuarioMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(UsuarioDto dto)
        {
            var e = UsuarioMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(UsuarioDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.UserId) ?? throw new KeyNotFoundException("User not found.");
            UsuarioMapper.CopyToExisting(e, dto);
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

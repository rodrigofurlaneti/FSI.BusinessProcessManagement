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
    public class ScreenAppService : IScreenAppService
    {
        private readonly IRepository<Screen> _repo;
        private readonly IUnitOfWork _uow;

        public ScreenAppService(IRepository<Screen> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<ScreenDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(ScreenMapper.ToDto);

        public async Task<ScreenDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : ScreenMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(ScreenDto dto)
        {
            var e = ScreenMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(ScreenDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.ScreenId) ?? throw new KeyNotFoundException("Screen not found.");
            ScreenMapper.CopyToExisting(e, dto);
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

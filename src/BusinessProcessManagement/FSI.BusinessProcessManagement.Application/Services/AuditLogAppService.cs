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
    public class AuditLogAppService : IAuditLogAppService
    {
        private readonly IRepository<AuditLog> _repo;
        private readonly IUnitOfWork _uow;

        public AuditLogAppService(IRepository<AuditLog> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(AuditLogMapper.ToDto);

        public async Task<AuditLogDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : AuditLogMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(AuditLogDto dto)
        {
            var e = AuditLogMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(AuditLogDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.AuditId) ?? throw new KeyNotFoundException("AuditLog not found.");
            AuditLogMapper.CopyToExisting(e, dto);
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

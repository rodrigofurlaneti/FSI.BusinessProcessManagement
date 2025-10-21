using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public class ProcessoBPMAppService : IProcessoBPMAppService
    {
        private readonly IRepository<DomainProcess> _repo;
        private readonly IUnitOfWork _uow;

        public ProcessoBPMAppService(IRepository<DomainProcess> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<ProcessoBPMDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(ProcessoBPMMapper.ToDto);

        public async Task<ProcessoBPMDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : ProcessoBPMMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(ProcessoBPMDto dto)
        {
            var e = ProcessoBPMMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(ProcessoBPMDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.ProcessId) ?? throw new KeyNotFoundException("Process not found.");
            ProcessoBPMMapper.CopyToExisting(e, dto);
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

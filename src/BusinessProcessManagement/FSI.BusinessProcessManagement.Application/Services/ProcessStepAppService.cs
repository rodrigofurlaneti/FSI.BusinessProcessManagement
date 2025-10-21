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
    public class ProcessStepAppService : IProcessStepAppService
    {
        private readonly IRepository<ProcessStep> _repo;
        private readonly IUnitOfWork _uow;

        public ProcessStepAppService(IRepository<ProcessStep> repo, IUnitOfWork uow)
        {
            _repo = repo;
            _uow = uow;
        }

        public async Task<IEnumerable<ProcessStepDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(ProcessStepMapper.ToDto);

        public async Task<ProcessStepDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : ProcessStepMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(ProcessStepDto dto)
        {
            var e = ProcessStepMapper.ToNewEntity(dto);
            await _repo.InsertAsync(e);
            await _uow.CommitAsync();
            return e.Id;
        }

        public async Task UpdateAsync(ProcessStepDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.StepId) ?? throw new KeyNotFoundException("Step not found.");
            ProcessStepMapper.CopyToExisting(e, dto);
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

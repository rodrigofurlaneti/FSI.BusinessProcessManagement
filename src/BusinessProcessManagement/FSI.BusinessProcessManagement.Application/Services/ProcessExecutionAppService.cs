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
    public class ProcessExecutionAppService : IProcessExecutionAppService
    {
        private readonly IRepository<ProcessExecution> _repo;
        private readonly IRepository<Process> _processRepo;
        private readonly IUnitOfWork _uow;

        public ProcessExecutionAppService(
            IRepository<ProcessExecution> repo,
            IRepository<Process> processRepo,
            IUnitOfWork uow)
        {
            _repo = repo;
            _processRepo = processRepo;
            _uow = uow;
        }

        public async Task<IEnumerable<ProcessExecutionDto>> GetAllAsync()
            => (await _repo.GetAllAsync()).Select(ProcessExecutionMapper.ToDto);

        public async Task<ProcessExecutionDto?> GetByIdAsync(long id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e is null ? null : ProcessExecutionMapper.ToDto(e);
        }

        public async Task<long> InsertAsync(ProcessExecutionDto dto)
        {
            // criação correta deve partir do agregado Process
            var process = await _processRepo.GetByIdAsync(dto.ProcessId)
                          ?? throw new KeyNotFoundException("Process not found.");
            var exec = process.StartExecution(dto.StepId, dto.UserId);
            ProcessExecutionMapper.CopyToExisting(exec, dto);

            await _repo.InsertAsync(exec);
            await _uow.CommitAsync();
            return exec.Id;
        }

        public async Task UpdateAsync(ProcessExecutionDto dto)
        {
            var e = await _repo.GetByIdAsync(dto.ExecutionId) ?? throw new KeyNotFoundException("Execution not found.");
            ProcessExecutionMapper.CopyToExisting(e, dto);
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

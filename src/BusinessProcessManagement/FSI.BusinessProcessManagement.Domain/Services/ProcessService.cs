namespace FSI.BusinessProcessManagement.Domain.Services
{
    public class ProcessService
    {
        private readonly IProcessRepository _processRepository;
        private readonly IProcessStepRepository _stepRepository;

        public ProcessService(IProcessRepository processRepository, IProcessStepRepository stepRepository)
        {
            _processRepository = processRepository;
            _stepRepository = stepRepository;
        }

        public async Task<bool> StartProcessAsync(long processId, long userId)
        {
            var process = await _processRepository.GetByIdAsync(processId);
            if (process == null)
                throw new InvalidOperationException("Processo não encontrado.");

            var steps = await _stepRepository.GetByProcessIdAsync(processId);
            if (!steps.Any())
                throw new InvalidOperationException("Processo sem etapas definidas.");

            process.Start(userId);
            await _processRepository.UpdateAsync(process);
            return true;
        }
    }
}

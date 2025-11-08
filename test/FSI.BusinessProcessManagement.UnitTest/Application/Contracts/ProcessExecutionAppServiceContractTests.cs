using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public class ProcessExecutionAppServiceContractTests
        : IGenericAppServiceContractTestsBase<ProcessExecutionDto>
    {
        protected override IGenericAppService<ProcessExecutionDto> CreateService()
        {
            long GetId(ProcessExecutionDto d) => d.ExecutionId;
            void SetId(ProcessExecutionDto d, long id) => d.ExecutionId = id;

            ProcessExecutionDto ApplyUpdate(ProcessExecutionDto current, ProcessExecutionDto incoming)
            {
                current.ProcessId = incoming.ProcessId;
                current.StepId = incoming.StepId;
                current.UserId = incoming.UserId;
                current.Status = incoming.Status;
                current.StartedAt = incoming.StartedAt;
                current.CompletedAt = incoming.CompletedAt;
                current.Remarks = incoming.Remarks;
                return current;
            }

            return new InMemoryGenericAppService<ProcessExecutionDto>(GetId, SetId, ApplyUpdate);
        }

        protected override ProcessExecutionDto CreateNewDto() => new ProcessExecutionDto
        {
            ProcessId = 100,
            StepId = 10,
            UserId = 1,
            Status = "Pendente",
            StartedAt = DateTime.UtcNow,
            Remarks = "Execução inicial criada"
        };

        protected override ProcessExecutionDto CreateUpdatedDto(ProcessExecutionDto original)
            => new ProcessExecutionDto
            {
                ExecutionId = original.ExecutionId,
                ProcessId = original.ProcessId,
                StepId = original.StepId,
                UserId = original.UserId,
                Status = "Concluído",
                StartedAt = original.StartedAt,
                CompletedAt = DateTime.UtcNow.AddMinutes(5),
                Remarks = "Processo concluído com sucesso"
            };

        protected override long GetId(ProcessExecutionDto dto) => dto.ExecutionId;
    }
}
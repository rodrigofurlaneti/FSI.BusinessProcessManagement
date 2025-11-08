using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Contracts;
using FSI.BusinessProcessManagement.UnitTests.Fakes;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class ProcessStepAppServiceContractTests
        : IGenericAppServiceContractTestsBase<ProcessStepDto>
    {
        protected override IGenericAppService<ProcessStepDto> CreateService()
        {
            long GetId(ProcessStepDto d) => d.StepId;
            void SetId(ProcessStepDto d, long id) => d.StepId = id;

            ProcessStepDto ApplyUpdate(ProcessStepDto current, ProcessStepDto incoming)
            {
                current.ProcessId = incoming.ProcessId;
                current.StepName = incoming.StepName;
                current.StepOrder = incoming.StepOrder;
                current.AssignedRoleId = incoming.AssignedRoleId;
                return current;
            }

            return new InMemoryGenericAppService<ProcessStepDto>(GetId, SetId, ApplyUpdate);
        }

        protected override ProcessStepDto CreateNewDto() => new ProcessStepDto
        {
            ProcessId = 1,
            StepName = "Validação Inicial",
            StepOrder = 1,
            AssignedRoleId = 10
        };

        protected override ProcessStepDto CreateUpdatedDto(ProcessStepDto original)
            => new ProcessStepDto
            {
                StepId = original.StepId, // mantém o mesmo Id
                ProcessId = original.ProcessId,
                StepName = "Aprovação Final",
                StepOrder = 2,
                AssignedRoleId = 20
            };

        protected override long GetId(ProcessStepDto dto) => dto.StepId;
    }
}

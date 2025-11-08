using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Fakes;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public class ProcessoBPMAppServiceContractTests
        : IGenericAppServiceContractTestsBase<ProcessoBPMDto>
    {
        protected override IGenericAppService<ProcessoBPMDto> CreateService()
        {
            long GetId(ProcessoBPMDto d) => d.ProcessId;
            void SetId(ProcessoBPMDto d, long id) => d.ProcessId = id;

            ProcessoBPMDto ApplyUpdate(ProcessoBPMDto current, ProcessoBPMDto incoming)
            {
                // mantém o mesmo Id e aplica alterações de campos
                current.DepartmentId = incoming.DepartmentId;
                current.ProcessName = incoming.ProcessName;
                current.Description = incoming.Description;
                current.CreatedBy = incoming.CreatedBy;
                return current;
            }

            return new InMemoryGenericAppService<ProcessoBPMDto>(GetId, SetId, ApplyUpdate);
        }

        protected override ProcessoBPMDto CreateNewDto() => new ProcessoBPMDto
        {
            DepartmentId = 20,
            ProcessName = "Aprovação de Contratos",
            Description = "Fluxo de aprovação de contratos de fornecedores",
            CreatedBy = 5
        };

        protected override ProcessoBPMDto CreateUpdatedDto(ProcessoBPMDto original)
            => new ProcessoBPMDto
            {
                ProcessId = original.ProcessId, // manter o mesmo Id!
                DepartmentId = original.DepartmentId,
                ProcessName = "Aprovação de Contratos (Revisado)",
                Description = "Fluxo revisado com novas regras de aprovação",
                CreatedBy = original.CreatedBy
            };

        protected override long GetId(ProcessoBPMDto dto) => dto.ProcessId;
    }
}

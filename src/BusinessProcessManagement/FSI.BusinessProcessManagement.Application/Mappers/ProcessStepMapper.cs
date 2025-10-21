using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class ProcessStepMapper
    {
        public static ProcessStep ToNewEntity(ProcessStepDto dto)
            => new ProcessStep(dto.ProcessId, dto.StepName ?? string.Empty, dto.StepOrder, dto.AssignedRoleId);

        public static void CopyToExisting(ProcessStep entity, ProcessStepDto dto)
        {
            entity.SetName(dto.StepName ?? string.Empty);
            entity.SetOrder(dto.StepOrder);
            entity.SetAssignedRole(dto.AssignedRoleId);
        }

        public static ProcessStepDto ToDto(ProcessStep entity) => new()
        {
            StepId = entity.Id,
            ProcessId = entity.ProcessId,
            StepName = entity.StepName,
            StepOrder = entity.StepOrder,
            AssignedRoleId = entity.AssignedRoleId
        };
    }
}

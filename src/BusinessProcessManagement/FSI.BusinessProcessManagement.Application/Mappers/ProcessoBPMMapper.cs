using FSI.BusinessProcessManagement.Application.Dtos;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class ProcessoBPMMapper
    {
        public static DomainProcess ToNewEntity(ProcessoBPMDto dto)
            => new DomainProcess(dto.ProcessName ?? string.Empty, dto.DepartmentId, dto.Description, dto.CreatedBy);

        public static void CopyToExisting(DomainProcess entity, ProcessoBPMDto dto)
        {
            entity.SetDescription(dto.Description);
            entity.SetDepartment(dto.DepartmentId);
            // nome do processo se quiser permitir renomeio:
            // entity.SetName(dto.ProcessName);
        }

        public static ProcessoBPMDto ToDto(DomainProcess entity) => new()
        {
            ProcessId = entity.Id,
            DepartmentId = entity.DepartmentId,
            ProcessName = entity.Name,
            Description = entity.Description,
            CreatedBy = entity.CreatedBy
        };
    }
}

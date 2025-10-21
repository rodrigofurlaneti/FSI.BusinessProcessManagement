using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class DepartmentMapper
    {
        public static Department ToNewEntity(DepartmentDto dto)
            => new Department(dto.DepartmentName ?? string.Empty, dto.Description);

        public static void CopyToExisting(Department entity, DepartmentDto dto)
        {
            entity.SetName(dto.DepartmentName ?? string.Empty);
            entity.SetDescription(dto.Description);
        }

        public static DepartmentDto ToDto(Department entity) => new()
        {
            DepartmentId = entity.Id,
            DepartmentName = entity.Name,
            Description = entity.Description
        };
    }
}

using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public class DepartmentAppService : GenericAppService<DepartmentDto, Department>
    {
        public DepartmentAppService(IRepository<Department> repo, IUnitOfWork uow)
            : base(repo, uow) { }

        protected override Department MapToEntity(DepartmentDto dto) =>
            new Department(dto.DepartmentId, dto.DepartmentName, dto.Description ?? "");

        protected override DepartmentDto MapToDto(Department entity) =>
            new DepartmentDto
            {
                DepartmentId = entity.DepartmentId,
                DepartmentName = entity.DepartmentName,
                Description = entity.Description
            };
    }
}

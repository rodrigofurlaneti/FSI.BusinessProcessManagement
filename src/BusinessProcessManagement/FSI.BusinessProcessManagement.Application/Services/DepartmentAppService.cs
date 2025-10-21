using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    /// <summary>
    /// AppService de Department com mapeamento DTO &lt;-&gt; Entidade
    /// e validações de negócio no Update.
    /// </summary>
    public class DepartmentAppService : GenericAppService<DepartmentDto, Department>, IDepartmentAppService
    {
        public DepartmentAppService(IUnitOfWork uow, IRepository<Department> repo)
            : base(uow, repo) { }

        /// <summary>
        /// Mapeia DTO -> Entidade. Usado no Insert (e no Update padrão, se não sobrescrito).
        /// </summary>
        protected override Department MapToEntity(DepartmentDto dto)
        {
            // A entidade Department tem construtor: Department(string name, string? description = null)
            var entity = new Department(
                name: dto.DepartmentName ?? string.Empty,
                description: dto.Description
            );

            // Se o DTO vier com Id > 0 (update via método padrão), injeta o Id (se necessário)
            if (dto.DepartmentId > 0)
            {
                var idProp = typeof(Department).GetProperty("Id");
                idProp?.SetValue(entity, dto.DepartmentId);
            }

            return entity;
        }

        /// <summary>
        /// Mapeia Entidade -> DTO. Usado em GetAll/GetById.
        /// </summary>
        protected override DepartmentDto MapToDto(Department entity)
        {
            return new DepartmentDto
            {
                DepartmentId = entity.Id,
                DepartmentName = entity.Name,
                Description = entity.Description
            };
        }

        /// <summary>
        /// Update com regra de negócio: carrega a entidade e aplica setters do domínio.
        /// </summary>
        public override async Task UpdateAsync(DepartmentDto dto)
        {
            var existing = await Repository.GetByIdAsync(dto.DepartmentId)
                           ?? throw new KeyNotFoundException("Department not found.");

            existing.SetName(dto.DepartmentName ?? string.Empty);
            existing.SetDescription(dto.Description);

            await Repository.UpdateAsync(existing);
            await Uow.CommitAsync();
        }
    }
}

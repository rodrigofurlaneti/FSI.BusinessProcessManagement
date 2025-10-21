using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public class UsuarioAppService : GenericAppService<UsuarioDto, User>, IUsuarioAppService
    {
        private readonly IRepository<Department> _deptRepo;

        public UsuarioAppService(
            IUnitOfWork uow,
            IRepository<User> repository,
            IRepository<Department> deptRepo) : base(uow, repository)
        {
            _deptRepo = deptRepo;
        }

        // === Mapeamentos exigidos pela classe base (nomes corretos) ===
        protected override UsuarioDto MapToDto(User entity)
        {
            return new UsuarioDto
            {
                UserId = entity.Id,
                DepartmentId = entity.DepartmentId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.IsActive,
                // Por segurança, nunca devolva PasswordHash no DTO em listagens/gets.
            };
        }

        protected override User MapToEntity(UsuarioDto dto)
        {
            // Validação de FK (departamento) se informado
            if (dto.DepartmentId.HasValue)
            {
                var depExists = _deptRepo.GetByIdAsync(dto.DepartmentId.Value)
                                         .GetAwaiter().GetResult();
                if (depExists == null)
                    throw new KeyNotFoundException("Department not found.");
            }

            // Regra: no Insert exigimos PasswordHash; no Update sobrescrevemos abaixo
            var passwordHash = dto.PasswordHash ?? string.Empty;
            if (dto.UserId == 0 && string.IsNullOrWhiteSpace(passwordHash))
                throw new System.ArgumentException("PasswordHash required for new user.");

            var user = new User(
                username: dto.Username ?? string.Empty,
                passwordHash: passwordHash,
                departmentId: dto.DepartmentId,
                email: dto.Email,
                isActive: dto.IsActive
            );

            // Se vier com Id (update via impl. padrão), mantemos
            if (dto.UserId > 0)
            {
                // seta via reflexão se sua entidade não expõe setter público
                var idProp = typeof(User).GetProperty("Id");
                idProp?.SetValue(user, dto.UserId);
            }

            return user;
        }

        // === Update com regras de negócio específicas ===
        public override async Task UpdateAsync(UsuarioDto dto)
        {
            var existing = await Repository.GetByIdAsync(dto.UserId)
                           ?? throw new KeyNotFoundException("User not found.");

            existing.SetUsername(dto.Username);
            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                existing.SetPasswordHash(dto.PasswordHash);

            existing.SetEmail(dto.Email);
            existing.SetDepartment(dto.DepartmentId);
            if (dto.IsActive) existing.Activate(); else existing.Deactivate();

            await Repository.UpdateAsync(existing);
            await Uow.CommitAsync();
        }
    }
}

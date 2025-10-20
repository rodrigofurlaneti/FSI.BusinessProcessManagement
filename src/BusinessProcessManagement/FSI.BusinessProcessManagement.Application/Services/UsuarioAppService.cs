using FSI.BusinessProcessManagement.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Application.Services
{
    public class UsuarioAppService : GenericAppService<UsuarioDto, User>, IUsuarioAppService
    {
        private readonly IRepository<Department> _deptRepo;
        public UsuarioAppService(IUnitOfWork uow, IRepository<User> repository, IRepository<Department> deptRepo)
            : base(uow, repository)
        {
            _deptRepo = deptRepo;
        }

        protected override UsuarioDto ToDto(User entity)
        {
            return new UsuarioDto
            {
                UserId = entity.Id,
                DepartmentId = entity.DepartmentId,
                Username = entity.Username,
                Email = entity.Email,
                IsActive = entity.IsActive
            };
        }

        protected override User FromDto(UsuarioDto dto)
        {
            if (dto.DepartmentId.HasValue)
            {
                var depExists = _deptRepo.GetByIdAsync(dto.DepartmentId.Value).GetAwaiter().GetResult();
                if (depExists == null) throw new KeyNotFoundException("Department not found.");
            }

            var user = new User(dto.Username, dto.PasswordHash ?? throw new System.ArgumentException("PasswordHash required"), dto.DepartmentId, dto.Email, dto.IsActive);
            return user;
        }

        public override async Task UpdateAsync(UsuarioDto dto)
        {
            var existing = await Repository.GetByIdAsync(dto.UserId) ?? throw new KeyNotFoundException("User not found.");
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

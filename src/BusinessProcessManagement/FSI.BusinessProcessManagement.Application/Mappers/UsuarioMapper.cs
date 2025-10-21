using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class UsuarioMapper
    {
        public static User ToNewEntity(UsuarioDto dto)
            => new User(dto.Username ?? string.Empty,
                        dto.PasswordHash ?? string.Empty,
                        dto.DepartmentId,
                        dto.Email,
                        dto.IsActive);

        public static void CopyToExisting(User entity, UsuarioDto dto)
        {
            entity.SetUsername(dto.Username);
            if (!string.IsNullOrWhiteSpace(dto.PasswordHash))
                entity.SetPasswordHash(dto.PasswordHash);
            entity.SetEmail(dto.Email);
            entity.SetDepartment(dto.DepartmentId);
            if (dto.IsActive) entity.Activate(); else entity.Deactivate();
        }

        public static UsuarioDto ToDto(User entity) => new()
        {
            UserId = entity.Id,
            DepartmentId = entity.DepartmentId,
            Username = entity.Username,
            Email = entity.Email,
            IsActive = entity.IsActive
        };
    }
}

namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class UsuarioDto
    {
        public long UserId { get; set; }
        public long? DepartmentId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

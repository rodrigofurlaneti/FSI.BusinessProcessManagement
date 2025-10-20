namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class RoleDto
    {
        public long RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

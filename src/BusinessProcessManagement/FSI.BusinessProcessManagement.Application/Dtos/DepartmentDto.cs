namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class DepartmentDto
    {
        public long DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}

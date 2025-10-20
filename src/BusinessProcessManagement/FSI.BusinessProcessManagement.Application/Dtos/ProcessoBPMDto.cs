namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class ProcessoBPMDto
    {
        public long ProcessId { get; set; }
        public long? DepartmentId { get; set; }
        public string ProcessName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public long? CreatedBy { get; set; }
    }
}

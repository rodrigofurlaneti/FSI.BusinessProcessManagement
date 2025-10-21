namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class ProcessExecutionDto
    {
        public long ExecutionId { get; set; }
        public long ProcessId { get; set; }
        public long StepId { get; set; }
        public long? UserId { get; set; }
        public string Status { get; set; } = "Pendente";
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public string? Remarks { get; set; }
    }
}

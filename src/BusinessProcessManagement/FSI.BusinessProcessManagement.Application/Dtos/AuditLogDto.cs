namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class AuditLogDto
    {
        public long AuditId { get; set; }
        public long? UserId { get; set; }
        public long? ScreenId { get; set; }
        public string ActionType { get; set; } = string.Empty;
        public DateTime ActionTimestamp { get; set; }
        public string? AdditionalInfo { get; set; }
    }
}

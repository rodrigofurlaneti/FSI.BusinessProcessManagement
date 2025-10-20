namespace FSI.BusinessProcessManagement.Application.Dtos
{
    public class RoleScreenPermissionDto
    {
        public long RoleScreenPermissionId { get; set; }
        public long RoleId { get; set; }
        public long ScreenId { get; set; }
        public bool CanView { get; set; }
        public bool CanCreate { get; set; }
        public bool CanEdit { get; set; }
        public bool CanDelete { get; set; }
    }
}

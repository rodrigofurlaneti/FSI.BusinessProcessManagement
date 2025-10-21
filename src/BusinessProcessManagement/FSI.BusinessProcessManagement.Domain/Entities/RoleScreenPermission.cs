using FSI.BusinessProcessManagement.Domain.Exceptions;
namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region RoleScreenPermission
    public sealed class RoleScreenPermission : BaseEntity
    {
        public long RoleId { get; private set; }
        public long ScreenId { get; private set; }
        public bool CanView { get; private set; }
        public bool CanCreate { get; private set; }
        public bool CanEdit { get; private set; }
        public bool CanDelete { get; private set; }
        private RoleScreenPermission() { }
        public RoleScreenPermission(long roleId, long screenId, bool canView = false, bool canCreate = false, bool canEdit = false, bool canDelete = false)
        {
            if (roleId <= 0) throw new DomainException("Invalid RoleId.");
            if (screenId <= 0) throw new DomainException("Invalid ScreenId.");
            RoleId = roleId;
            ScreenId = screenId;
            CanView = canView;
            CanCreate = canCreate;
            CanEdit = canEdit;
            CanDelete = canDelete;
        }
        public void SetPermissions(bool? view = null, bool? create = null, bool? edit = null, bool? delete = null)
        {
            if (view.HasValue) CanView = view.Value;
            if (create.HasValue) CanCreate = create.Value;
            if (edit.HasValue) CanEdit = edit.Value;
            if (delete.HasValue) CanDelete = delete.Value;
            Touch();
        }
    }
    #endregion
}

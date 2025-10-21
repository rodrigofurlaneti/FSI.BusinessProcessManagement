using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class RoleScreenPermission : BaseEntity
    {
        public long RoleId { get; private set; }
        public long ScreenId { get; private set; }
        public bool CanView { get; private set; }
        public bool CanCreate { get; private set; }
        public bool CanEdit { get; private set; }
        public bool CanDelete { get; private set; }

        private RoleScreenPermission() { }

        public RoleScreenPermission(long roleId, long screenId, bool canView, bool canCreate, bool canEdit, bool canDelete)
        {
            SetRole(roleId);
            SetScreen(screenId);
            SetPermissions(canView, canCreate, canEdit, canDelete);
        }

        public void SetRole(long roleId)
        {
            if (roleId <= 0) throw new DomainException("Invalid role.");
            RoleId = roleId;
            Touch();
        }

        public void SetScreen(long screenId)
        {
            if (screenId <= 0) throw new DomainException("Invalid screen.");
            ScreenId = screenId;
            Touch();
        }

        public void SetPermissions(bool view, bool create, bool edit, bool delete)
        {
            CanView = view;
            CanCreate = create;
            CanEdit = edit;
            CanDelete = delete;
            Touch();
        }
    }
}

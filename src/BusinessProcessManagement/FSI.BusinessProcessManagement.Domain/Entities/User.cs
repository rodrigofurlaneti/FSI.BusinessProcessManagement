using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region User


    public sealed class User : BaseEntity
    {
        public long? DepartmentId { get; private set; }
        public string Username { get; private set; }
        public string PasswordHash { get; private set; }
        public string? Email { get; private set; }
        public bool IsActive { get; private set; }
        private readonly List<UserRole> _userRoles = new();
        public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();
        private User() { }
        public User(string username, string passwordHash, long? departmentId = null, string? email = null, bool isActive = true)
        {
            SetUsername(username);
            SetPasswordHash(passwordHash);
            SetDepartment(departmentId);
            SetEmail(email);
            IsActive = isActive;
        }
        public void SetUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new DomainException("Username is required.");
            if (username.Length > 100) throw new DomainException("Username too long (max 100).");
            Username = username.Trim();
            Touch();
        }


        public void SetPasswordHash(string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new DomainException("PasswordHash is required (store hashes only).");
            if (passwordHash.Length > 255)
                throw new DomainException("PasswordHash too long (max 255).");


            PasswordHash = passwordHash;
            Touch();
        }


        public void SetEmail(string? email)
        {
            if (email == null) { Email = null; Touch(); return; }
            var trimmed = email.Trim();
            if (trimmed.Length > 200) throw new DomainException("Email too long (max 200).");
            // Basic email format check (simple)
            if (!trimmed.Contains('@')) throw new DomainException("Email format invalid.");
            Email = trimmed;
            Touch();
        }


        public void SetDepartment(long? departmentId)
        {
            DepartmentId = departmentId;
            Touch();
        }


        public void Activate()
        {
            if (IsActive) return;
            IsActive = true;
            Touch();
        }


        public void Deactivate()
        {
            if (!IsActive) return;
            IsActive = false;
            Touch();
        }


        // Role association helpers (domain-level convenience methods)
        public void AddRole(Role role)
        {
            if (role == null) throw new ArgumentNullException(nameof(role));
            if (_userRoles.Any(ur => ur.RoleId == role.Id))
                throw new DomainException("User already has this role.");


            _userRoles.Add(new UserRole(this.Id, role.Id));
            Touch();
        }


        public void RemoveRole(long roleId)
        {
            var ur = _userRoles.FirstOrDefault(x => x.RoleId == roleId);
            if (ur == null) throw new DomainException("Role not assigned to user.");
            _userRoles.Remove(ur);
            Touch();
        }

        #endregion
    }
}

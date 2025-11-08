using System;
using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class UserRole : BaseEntity
    {
        public long UserId { get; private set; }
        public long RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }
        public User User { get; private set; } = null!;
        public Role Role { get; private set; } = null!;

        private UserRole() { }

        public UserRole(long userId, long roleId)
        {
            SetUser(userId);
            SetRole(roleId);
            AssignedAt = DateTime.UtcNow;
        }

        public static UserRole Create(User user, Role role)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (role is null) throw new ArgumentNullException(nameof(role));

            var link = new UserRole(user.Id, role.Id);
            link.SetUserNavigation(user);
            link.SetRoleNavigation(role);
            return link;
        }

        public void SetUser(long userId)
        {
            if (userId <= 0) throw new DomainException("Invalid UserId.");
            UserId = userId;
            Touch();
        }

        public void SetRole(long roleId)
        {
            if (roleId <= 0) throw new DomainException("Invalid RoleId.");
            RoleId = roleId;
            Touch();
        }

        public void SetUserNavigation(User user)
        {
            if (user is null) throw new ArgumentNullException(nameof(user));
            if (user.Id <= 0) throw new DomainException("Invalid UserId.");
            User = user;
            if (UserId != user.Id) SetUser(user.Id);
        }

        public void SetRoleNavigation(Role role)
        {
            if (role is null) throw new ArgumentNullException(nameof(role));
            if (role.Id <= 0) throw new DomainException("Invalid RoleId.");
            Role = role;
            if (RoleId != role.Id) SetRole(role.Id);
        }
    }
}

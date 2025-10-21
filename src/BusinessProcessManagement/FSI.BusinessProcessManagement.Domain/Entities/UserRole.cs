using System;
using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class UserRole : BaseEntity
    {
        public long UserId { get; private set; }
        public long RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }

        private UserRole() { }

        public UserRole(long userId, long roleId)
        {
            SetUser(userId);
            SetRole(roleId);
            AssignedAt = DateTime.UtcNow;
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
    }
}

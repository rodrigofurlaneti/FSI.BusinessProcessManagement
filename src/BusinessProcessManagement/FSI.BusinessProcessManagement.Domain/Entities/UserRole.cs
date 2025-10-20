using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region UserRole


    public sealed class UserRole : Entity
    {
        public long UserId { get; private set; }
        public long RoleId { get; private set; }
        public DateTime AssignedAt { get; private set; }


        private UserRole() { }


        public UserRole(long userId, long roleId)
        {
            if (userId <= 0) throw new DomainException("Invalid UserId.");
            if (roleId <= 0) throw new DomainException("Invalid RoleId.");
            UserId = userId;
            RoleId = roleId;
            AssignedAt = DateTime.UtcNow;
        }


        public void Reassign(long newUserId, long newRoleId)
        {
            if (newUserId <= 0 || newRoleId <= 0) throw new DomainException("Invalid ids for reassignment.");
            UserId = newUserId;
            RoleId = newRoleId;
            AssignedAt = DateTime.UtcNow;
            Touch();
        }
    }


    #endregion
}

using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.ValueObjects;

namespace FSI.BusinessProcessManagement.Domain.Services
{
    public class UserService
    {
        private readonly IUnitOfWork _uow;
        public UserService(IUnitOfWork uow) => _uow = uow;
        public async Task<User> CreateUserAsync(string username, PasswordHash passwordHash, long? departmentId = null, Email? email = null)
        {
            if (!string.IsNullOrWhiteSpace(username)) username = username.Trim();
            if (departmentId.HasValue)
            {
                var dep = await _uow.Departments.GetByIdAsync(departmentId.Value);
                if (dep == null) throw new InvalidOperationException("Department not found.");
            }
            var existing = await _uow.Users.GetByUsernameAsync(username);
            if (existing != null) throw new InvalidOperationException("Username already exists.");
            var user = new User(username, passwordHash.Hash, departmentId, email?.Address);
            await _uow.Users.InsertAsync(user);
            await _uow.CommitAsync();
            return user;
        }

        public async Task AssignRoleAsync(long userId, long roleId)
        {
            var user = await _uow.Users.GetByIdAsync(userId) ?? throw new InvalidOperationException("User not found.");
            var role = await _uow.Roles.GetByIdAsync(roleId) ?? throw new InvalidOperationException("Role not found.");
            var ur = new UserRole(user.Id, role.Id);
            await _uow.UserRoles.InsertAsync(ur);
            await _uow.CommitAsync();
        }

        public async Task RemoveRoleAsync(long userRoleId)
        {
            await _uow.UserRoles.DeleteAsync(userRoleId);
            await _uow.CommitAsync();
        }
    }
}

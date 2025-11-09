using FSI.BusinessProcessManagement.Domain.Entities;
using System;
using System.Reflection;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Helpers
{
    public static class Api
    {
        public static T WithPrivateId<T>(T entity, long id)
        {
            var prop = entity!.GetType().GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop is null) throw new InvalidOperationException("Propriedade Id não encontrada para set via reflexão.");
            prop.SetValue(entity, id, null);
            return entity;
        }

        public static User MakeUser(long id, string username, string passwordHash, bool isActive = true)
        {
            var u = new User(username, passwordHash, null, null, isActive);
            return WithPrivateId(u, id);
        }

        public static Role MakeRole(long id, string name, string? description = null)
        {
            var r = new Role(name, description);
            return WithPrivateId(r, id);
        }

        public static UserRole MakeUserRole(long userId, long roleId) => new UserRole(userId, roleId);
    }
}

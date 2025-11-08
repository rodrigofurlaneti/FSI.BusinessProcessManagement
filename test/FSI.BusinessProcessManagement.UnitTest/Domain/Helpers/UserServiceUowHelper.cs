using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Helpers
{
    public static class UserServiceUowHelper
    {
        public static Mock<IUnitOfWork> NewUowMock(
            out Mock<IDepartmentRepository> departments,
            out Mock<IUserRepository> users,
            out Mock<IRoleRepository> roles,
            out Mock<IUserRoleRepository> userRoles)
        {
            var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

            departments = new Mock<IDepartmentRepository>(MockBehavior.Strict);
            users = new Mock<IUserRepository>(MockBehavior.Strict);
            roles = new Mock<IRoleRepository>(MockBehavior.Strict);
            userRoles = new Mock<IUserRoleRepository>(MockBehavior.Strict);

            uow.SetupGet(x => x.Departments).Returns(departments.Object);
            uow.SetupGet(x => x.Users).Returns(users.Object);
            uow.SetupGet(x => x.Roles).Returns(roles.Object);
            uow.SetupGet(x => x.UserRoles).Returns(userRoles.Object);

            return uow;
        }

        public static void SetId(object entity, long id)
        {
            var p = typeof(BaseEntity).GetProperty("Id",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            p!.SetValue(entity, id);
        }
    }
}

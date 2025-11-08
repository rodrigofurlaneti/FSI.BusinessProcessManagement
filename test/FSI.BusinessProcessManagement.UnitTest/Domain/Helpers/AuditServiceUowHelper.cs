using System;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Services;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Services
{
    internal static class AuditServiceUowHelper
    {
        public static Mock<IUnitOfWork> NewUowMock(
            out Mock<IUserRepository> users,
            out Mock<IScreenRepository> screens,
            out Mock<IAuditLogRepository> auditLogs)
        {
            var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

            users = new Mock<IUserRepository>(MockBehavior.Strict);
            screens = new Mock<IScreenRepository>(MockBehavior.Strict);
            auditLogs = new Mock<IAuditLogRepository>(MockBehavior.Strict);

            uow.SetupGet(x => x.Users).Returns(users.Object);
            uow.SetupGet(x => x.Screens).Returns(screens.Object);
            uow.SetupGet(x => x.AuditLogs).Returns(auditLogs.Object);

            return uow;
        }
    }
}
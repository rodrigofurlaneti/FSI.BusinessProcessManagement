// test/FSI.BusinessProcessManagement.UnitTests/Domain/Services/AuditServiceTests.cs
using System;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Services;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Services
{
    public class AuditServiceTests
    {
     
        [Fact]
        public async Task CreateLogAsync_Happy_NoUser_NoScreen_ShouldInsertAndCommit()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            AuditLog? captured = null;
            logs.Setup(r => r.InsertAsync(It.IsAny<AuditLog>()))
                .Callback<AuditLog>(a => captured = a)
                .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new AuditService(uow.Object);

            var result = await sut.CreateLogAsync(
                actionType: "LOGIN",
                userId: null,
                screenId: null,
                additionalInfo: "ip=127.0.0.1");

            Assert.NotNull(result);
            Assert.Same(captured, result);

            users.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            screens.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateLogAsync_Happy_WithUserOnly_ShouldInsertAndCommit()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            users.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new User("u", "hash"));

            logs.Setup(r => r.InsertAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new AuditService(uow.Object);

            var result = await sut.CreateLogAsync(
                actionType: "OPEN",
                userId: 10,
                screenId: null,
                additionalInfo: null);

            Assert.NotNull(result);

            users.Verify(r => r.GetByIdAsync(10), Times.Once);
            screens.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateLogAsync_Happy_WithScreenOnly_ShouldInsertAndCommit()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            screens.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(new Screen("Home")); // ajuste se sua entidade difere

            logs.Setup(r => r.InsertAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new AuditService(uow.Object);

            var result = await sut.CreateLogAsync(
                actionType: "NAV",
                userId: null,
                screenId: 7,
                additionalInfo: "menu=main");

            Assert.NotNull(result);

            users.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            screens.Verify(r => r.GetByIdAsync(7), Times.Once);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateLogAsync_Happy_WithUserAndScreen_ShouldInsertAndCommit()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            users.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User("u", "h"));
            screens.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(new Screen("Dash"));

            logs.Setup(r => r.InsertAsync(It.IsAny<AuditLog>())).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new AuditService(uow.Object);

            var result = await sut.CreateLogAsync(
                actionType: "CLICK",
                userId: 1,
                screenId: 2,
                additionalInfo: "btn=export");

            Assert.NotNull(result);

            users.Verify(r => r.GetByIdAsync(1), Times.Once);
            screens.Verify(r => r.GetByIdAsync(2), Times.Once);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        // ------------------------- Error paths -------------------------

        [Fact]
        public async Task CreateLogAsync_UserNotFound_ShouldThrow_AndNotInsert()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            users.Setup(r => r.GetByIdAsync(11)).ReturnsAsync((User?)null);

            var sut = new AuditService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.CreateLogAsync("ANY", userId: 11, screenId: null, additionalInfo: null));

            Assert.Equal("User not found.", ex.Message);

            users.Verify(r => r.GetByIdAsync(11), Times.Once);
            screens.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateLogAsync_ScreenNotFound_ShouldThrow_AndNotInsert()
        {
            var uow = AuditServiceUowHelper.NewUowMock(out var users, out var screens, out var logs);

            screens.Setup(r => r.GetByIdAsync(33)).ReturnsAsync((Screen?)null);

            var sut = new AuditService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.CreateLogAsync("ANY", userId: null, screenId: 33, additionalInfo: null));

            Assert.Equal("Screen not found.", ex.Message);

            screens.Verify(r => r.GetByIdAsync(33), Times.Once);
            users.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            logs.Verify(r => r.InsertAsync(It.IsAny<AuditLog>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }
    }
}

using System;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.UnitTests.Domain.Helpers;
using FSI.BusinessProcessManagement.Domain.Services;
using FSI.BusinessProcessManagement.Domain.ValueObjects;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Services
{
    public class UserServiceTests
    {
        // ------------------------- CreateUserAsync -------------------------

        [Fact]
        public async Task CreateUserAsync_Happy_Minimal_ShouldInsertAndCommit()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            // username não existe
            users.Setup(r => r.GetByUsernameAsync("john")).ReturnsAsync((User?)null);

            User? captured = null;
            users.Setup(r => r.InsertAsync(It.IsAny<User>()))
                 .Callback<User>(u => captured = u)
                 .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new UserService(uow.Object);

            var result = await sut.CreateUserAsync(
                username: "john",
                passwordHash: new PasswordHash("HASH123"),
                departmentId: null,
                email: null);

            Assert.NotNull(result);
            Assert.Same(captured, result);
            Assert.Equal("john", result.Username);
            Assert.Null(result.DepartmentId);
            Assert.Null(result.Email);
            Assert.True(result.IsActive);

            users.Verify(r => r.GetByUsernameAsync("john"), Times.Once);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
            deps.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUserAsync_Happy_WithDepartment_ShouldInsertAndCommit()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            // departamento existe
            deps.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(new Department("D"));

            // username livre (serviço trimma " joao ")
            users.Setup(r => r.GetByUsernameAsync("joao")).ReturnsAsync((User?)null);

            users.Setup(r => r.InsertAsync(It.IsAny<User>())).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new UserService(uow.Object);

            var result = await sut.CreateUserAsync(
                username: "  joao  ",
                passwordHash: new PasswordHash("H"),
                departmentId: 10,
                email: null);

            Assert.NotNull(result);
            Assert.Equal(10, result.DepartmentId);
            Assert.Equal("joao", result.Username);

            deps.Verify(r => r.GetByIdAsync(10), Times.Once);
            users.Verify(r => r.GetByUsernameAsync("joao"), Times.Once);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUserAsync_Happy_WithEmail_ShouldInsertAndCommit()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            users.Setup(r => r.GetByUsernameAsync("ana")).ReturnsAsync((User?)null);

            User? captured = null;
            users.Setup(r => r.InsertAsync(It.IsAny<User>()))
                 .Callback<User>(u => captured = u)
                 .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new UserService(uow.Object);

            var email = new Email("  ana@example.com  "); // se o VO já trimma internamente, ok
            var result = await sut.CreateUserAsync("ana", new PasswordHash("HASH"), null, email);

            Assert.NotNull(result);
            Assert.Same(captured, result);
            Assert.Equal("ana@example.com", result.Email);

            users.Verify(r => r.GetByUsernameAsync("ana"), Times.Once);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Once);
            deps.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUserAsync_DepartmentNotFound_ShouldThrow()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            deps.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department?)null);

            var sut = new UserService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.CreateUserAsync("user", new PasswordHash("H"), departmentId: 99, email: null));

            Assert.Equal("Department not found.", ex.Message);

            deps.Verify(r => r.GetByIdAsync(99), Times.Once);
            users.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUserAsync_UsernameAlreadyExists_ShouldThrow()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            // não tem departamento
            // username já existe
            users.Setup(r => r.GetByUsernameAsync("alice")).ReturnsAsync(new User("alice", "HASH"));

            var sut = new UserService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.CreateUserAsync("alice", new PasswordHash("H"), null, null));

            Assert.Equal("Username already exists.", ex.Message);

            users.Verify(r => r.GetByUsernameAsync("alice"), Times.Once);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateUserAsync_BlankUsername_ShouldPropagateDomainValidation()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var deps, out var users, out var roles, out var userRoles);

            // username em branco → GetByUsernameAsync vai receber "   " (serviço não trimma quando whitespace)
            users.Setup(r => r.GetByUsernameAsync("   ")).ReturnsAsync((User?)null);

            var sut = new UserService(uow.Object);

            // Espera a validação do próprio User (Domain) explodir
            await Assert.ThrowsAsync<FSI.BusinessProcessManagement.Domain.Exceptions.DomainException>(() =>
                sut.CreateUserAsync("   ", new PasswordHash("H"), null, null));

            users.Verify(r => r.GetByUsernameAsync("   "), Times.Once);
            users.Verify(r => r.InsertAsync(It.IsAny<User>()), Times.Never);
            deps.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        // ------------------------- AssignRoleAsync -------------------------

        [Fact]
        public async Task AssignRoleAsync_Happy_ShouldInsertUserRole_AndCommit()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var dep, out var usr, out var rol, out var userRoles);

            var user = new User("u", "h");
            UserServiceUowHelper.SetId(user, 1);  // ← importante

            var role = new Role("Admin");
            UserServiceUowHelper.SetId(role, 2);  // ← importante

            usr.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);
            rol.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(role);

            UserRole? captured = null;
            userRoles.Setup(r => r.InsertAsync(It.IsAny<UserRole>()))
                     .Callback<UserRole>(ur => captured = ur)
                     .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new UserService(uow.Object);

            await sut.AssignRoleAsync(1, 2);

            Assert.NotNull(captured);
            Assert.Equal(1, captured!.UserId);
            Assert.Equal(2, captured.RoleId);

            usr.Verify(r => r.GetByIdAsync(1), Times.Once);
            rol.Verify(r => r.GetByIdAsync(2), Times.Once);
            userRoles.Verify(r => r.InsertAsync(It.IsAny<UserRole>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            dep.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            uow.VerifyNoOtherCalls();
        }



        [Fact]
        public async Task AssignRoleAsync_UserNotFound_ShouldThrow()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var dep, out var usr, out var rol, out var userRoles);

            usr.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((User?)null);

            var sut = new UserService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.AssignRoleAsync(1, 2));

            Assert.Equal("User not found.", ex.Message);

            usr.Verify(r => r.GetByIdAsync(1), Times.Once);
            rol.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            userRoles.Verify(r => r.InsertAsync(It.IsAny<UserRole>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AssignRoleAsync_RoleNotFound_ShouldThrow()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var dep, out var usr, out var rol, out var userRoles);

            usr.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new User("u", "h"));
            rol.Setup(r => r.GetByIdAsync(2)).ReturnsAsync((Role?)null);

            var sut = new UserService(uow.Object);

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                sut.AssignRoleAsync(1, 2));

            Assert.Equal("Role not found.", ex.Message);

            usr.Verify(r => r.GetByIdAsync(1), Times.Once);
            rol.Verify(r => r.GetByIdAsync(2), Times.Once);
            userRoles.Verify(r => r.InsertAsync(It.IsAny<UserRole>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        // ------------------------- RemoveRoleAsync -------------------------

        [Fact]
        public async Task RemoveRoleAsync_Happy_ShouldDeleteAndCommit()
        {
            var uow = UserServiceUowHelper.NewUowMock(out var dep, out var usr, out var rol, out var userRoles);

            userRoles.Setup(r => r.DeleteAsync(42)).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new UserService(uow.Object);

            await sut.RemoveRoleAsync(42);

            userRoles.Verify(r => r.DeleteAsync(42), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);

            dep.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            usr.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);
            rol.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never);

            uow.VerifyNoOtherCalls();
        }
    }
}

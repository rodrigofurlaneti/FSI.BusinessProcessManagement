using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests; // Para TestHelpers.SetId
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class UserRoleAppServiceTests
    {
        private readonly Mock<IRepository<UserRole>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private UserRoleAppService CreateSut()
            => new UserRoleAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var ur1 = new UserRole(userId: 1, roleId: 10);
            var ur2 = new UserRole(userId: 2, roleId: 20);
            TestHelpers.SetId(ur1, 1);
            TestHelpers.SetId(ur2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { ur1, ur2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.UserRoleId == 1);
            Assert.Contains(list, d => d.UserRoleId == 2);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((UserRole)null!);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(99);

            // Assert
            Assert.Null(dto);
            _repoMock.Verify(r => r.GetByIdAsync(99), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenFound()
        {
            // Arrange
            var entity = new UserRole(userId: 5, roleId: 10);
            TestHelpers.SetId(entity, 11);

            _repoMock.Setup(r => r.GetByIdAsync(11))
                     .ReturnsAsync(entity);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(11);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(11, dto!.UserRoleId);
            Assert.Equal(5, dto.UserId);
            Assert.Equal(10, dto.RoleId);

            _repoMock.Verify(r => r.GetByIdAsync(11), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new UserRoleDto { UserRoleId = 0, UserId = 1, RoleId = 2 };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<UserRole>()))
                     .Callback<UserRole>(e => TestHelpers.SetId(e, 99))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(99, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<UserRole>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenUserRoleNotFound()
        {
            // Arrange
            var dto = new UserRoleDto { UserRoleId = 77, UserId = 1, RoleId = 2 };
            _repoMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((UserRole)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("UserRole not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<UserRole>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new UserRole(userId: 1, roleId: 2);
            TestHelpers.SetId(entity, 50);
            var dto = new UserRoleDto { UserRoleId = 50, UserId = 3, RoleId = 4 };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<UserRole>(e => e == entity)), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAndCommit()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(9)).Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(9);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(9), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

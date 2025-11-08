using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class RoleScreenPermissionAppServiceTests
    {
        private readonly Mock<IRepository<RoleScreenPermission>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private RoleScreenPermissionAppService CreateSut()
            => new RoleScreenPermissionAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var p1 = new RoleScreenPermission(roleId: 1, screenId: 2, canView: true, canEdit: false, canDelete: false, canCreate: true);
            var p2 = new RoleScreenPermission(roleId: 2, screenId: 3, canView: true, canEdit: true, canDelete: true, canCreate: false);
            TestHelpers.SetId(p1, 10);
            TestHelpers.SetId(p2, 20);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { p1, p2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.RoleScreenPermissionId == 10);
            Assert.Contains(list, d => d.RoleScreenPermissionId == 20);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((RoleScreenPermission)null!);

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
            var permission = new RoleScreenPermission(roleId: 1, screenId: 5, canView: true, canEdit: false, canDelete: false, canCreate: false);
            TestHelpers.SetId(permission, 15);

            _repoMock.Setup(r => r.GetByIdAsync(15))
                     .ReturnsAsync(permission);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(15);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(15, dto!.RoleScreenPermissionId);
            Assert.True(dto.CanView);
            Assert.False(dto.CanEdit);

            _repoMock.Verify(r => r.GetByIdAsync(15), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleScreenPermissionId = 0,
                RoleId = 1,
                ScreenId = 2,
                CanView = true,
                CanEdit = false,
                CanCreate = true,
                CanDelete = false
            };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<RoleScreenPermission>()))
                     .Callback<RoleScreenPermission>(e => TestHelpers.SetId(e, 77))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(77, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<RoleScreenPermission>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenPermissionNotFound()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleScreenPermissionId = 99,
                RoleId = 1,
                ScreenId = 1,
                CanView = true
            };

            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((RoleScreenPermission)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Permission not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(99), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<RoleScreenPermission>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new RoleScreenPermission(roleId: 1, screenId: 3, canView: true, canEdit: false, canDelete: false, canCreate: false);
            TestHelpers.SetId(entity, 50);

            var dto = new RoleScreenPermissionDto
            {
                RoleScreenPermissionId = 50,
                RoleId = 1,
                ScreenId = 3,
                CanView = true,
                CanEdit = true, // alteração
                CanDelete = false,
                CanCreate = false
            };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<RoleScreenPermission>(e => e == entity)), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAndCommit()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(7)).Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(7);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(7), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

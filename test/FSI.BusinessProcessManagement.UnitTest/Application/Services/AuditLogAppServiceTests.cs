using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FSI.BusinessProcessManagement.Application.Tests.Services
{
    public class AuditLogAppServiceTests
    {
        private readonly Mock<IRepository<AuditLog>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private AuditLogAppService CreateSut()
            => new AuditLogAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            var a1 = new AuditLog(actionType: "CREATE", userId: 1, screenId: 10, additionalInfo: "ok");
            var a2 = new AuditLog(actionType: "UPDATE", userId: 2, screenId: 20, additionalInfo: "ok");
            TestHelpers.SetId(a1, 1);
            TestHelpers.SetId(a2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { a1, a2 }.AsEnumerable());

            var sut = CreateSut();

            var result = await sut.GetAllAsync();

            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.AuditId == 1);
            Assert.Contains(list, d => d.AuditId == 2);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((AuditLog)null!);

            var sut = CreateSut();

            var dto = await sut.GetByIdAsync(99);

            Assert.Null(dto);
            _repoMock.Verify(r => r.GetByIdAsync(99), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenFound()
        {
            var entity = new AuditLog("CREATE", 7, 1, "any");
            TestHelpers.SetId(entity, 10);

            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(entity);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(10);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.AuditId);

            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenEntityNotFound()
        {
            // Arrange
            var dto = new AuditLogDto { AuditId = 42 };

            _repoMock.Setup(r => r.GetByIdAsync(42)).ReturnsAsync((AuditLog)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("AuditLog not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(42), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<AuditLog>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
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

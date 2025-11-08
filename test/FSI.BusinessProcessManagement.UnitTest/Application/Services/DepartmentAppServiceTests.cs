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
    public class DepartmentAppServiceTests
    {
        private readonly Mock<IRepository<Department>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private DepartmentAppService CreateSut()
            => new DepartmentAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var d1 = new Department("TI", "Tecnologia da Informação");
            var d2 = new Department("Financeiro", "Gestão de Custos");
            TestHelpers.SetId(d1, 1);
            TestHelpers.SetId(d2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { d1, d2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.DepartmentId == 1);
            Assert.Contains(list, d => d.DepartmentId == 2);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Department)null!);
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
            var entity = new Department("RH", "Gestão de Pessoas");
            TestHelpers.SetId(entity, 10);

            _repoMock.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(entity);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(10);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.DepartmentId);

            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new DepartmentDto { DepartmentId = 0, DepartmentName = "Comercial", Description = "Área de vendas" };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<Department>()))
                     .Callback<Department>(e => TestHelpers.SetId(e, 99))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(99, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<Department>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDepartmentNotFound()
        {
            // Arrange
            var dto = new DepartmentDto { DepartmentId = 123, DepartmentName = "Jurídico", Description = "Área legal" };
            _repoMock.Setup(r => r.GetByIdAsync(123)).ReturnsAsync((Department)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Department not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(123), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Department>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new Department("Financeiro", "Gestão financeira");
            TestHelpers.SetId(entity, 50);
            var dto = new DepartmentDto { DepartmentId = 50, DepartmentName = "Financeiro", Description = "Atualizado" };

            _repoMock.Setup(r => r.GetByIdAsync(50)).ReturnsAsync(entity);
            _repoMock.Setup(r => r.UpdateAsync(entity)).Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Department>(e => e == entity)), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAndCommit()
        {
            // Arrange
            _repoMock.Setup(r => r.DeleteAsync(5)).Returns(Task.CompletedTask);
            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(5);

            // Assert
            _repoMock.Verify(r => r.DeleteAsync(5), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}

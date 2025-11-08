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
    public class ScreenAppServiceTests
    {
        private readonly Mock<IRepository<Screen>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private ScreenAppService CreateSut()
            => new ScreenAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var s1 = new Screen("Dashboard", "Visão geral do sistema");
            var s2 = new Screen("Relatórios", "Consulta de dados analíticos");
            TestHelpers.SetId(s1, 1);
            TestHelpers.SetId(s2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { s1, s2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.ScreenId == 1);
            Assert.Contains(list, d => d.ScreenId == 2);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((Screen)null!);

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
            var screen = new Screen("Usuários", "Gerenciamento de usuários");
            TestHelpers.SetId(screen, 10);

            _repoMock.Setup(r => r.GetByIdAsync(10))
                     .ReturnsAsync(screen);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(10);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.ScreenId);
            Assert.Equal("Usuários", dto!.ScreenName);

            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new ScreenDto { ScreenId = 0, ScreenName = "Configurações", Description = "Tela de configuração do sistema" };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<Screen>()))
                     .Callback<Screen>(e => TestHelpers.SetId(e, 99))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(99, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<Screen>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenScreenNotFound()
        {
            // Arrange
            var dto = new ScreenDto { ScreenId = 77, ScreenName = "Atualizar Tela" };
            _repoMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((Screen)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Screen not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Screen>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new Screen("Home", "Tela inicial");
            TestHelpers.SetId(entity, 50);
            var dto = new ScreenDto { ScreenId = 50, ScreenName = "Home Atualizada", Description = "Tela inicial revisada" };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<Screen>(e => e == entity)), Times.Once);
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

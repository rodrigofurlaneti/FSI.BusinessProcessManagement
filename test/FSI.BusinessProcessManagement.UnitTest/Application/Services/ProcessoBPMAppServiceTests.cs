using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests; // onde está o TestHelpers
using Moq;
using Xunit;
// Alias da entidade de domínio:
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class ProcessoBPMAppServiceTests
    {
        private readonly Mock<IRepository<DomainProcess>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private ProcessoBPMAppService CreateSut()
            => new ProcessoBPMAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var p1 = new DomainProcess("Cadastro de Funcionários");
            var p2 = new DomainProcess("Aprovação de Contratos");
            TestHelpers.SetId(p1, 1);
            TestHelpers.SetId(p2, 2);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { p1, p2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.ProcessId == 1);
            Assert.Contains(list, d => d.ProcessId == 2);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((DomainProcess)null!);

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
            var process = new DomainProcess("Gestão de Viagens");
            TestHelpers.SetId(process, 10);

            _repoMock.Setup(r => r.GetByIdAsync(10))
                     .ReturnsAsync(process);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(10);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.ProcessId);

            _repoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new ProcessoBPMDto { ProcessId = 0, ProcessName = "Novo Processo" };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<DomainProcess>()))
                     .Callback<DomainProcess>(e => TestHelpers.SetId(e, 99))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(99, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<DomainProcess>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenProcessNotFound()
        {
            // Arrange
            var dto = new ProcessoBPMDto { ProcessId = 77, ProcessName = "Atualizar Processo" };
            _repoMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((DomainProcess)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Process not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<DomainProcess>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new DomainProcess("Processo Antigo");
            TestHelpers.SetId(entity, 50);
            var dto = new ProcessoBPMDto { ProcessId = 50, ProcessName = "Processo Atualizado" };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<DomainProcess>(e => e == entity)), Times.Once);
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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests; // para TestHelpers.SetId
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class ProcessStepAppServiceTests
    {
        private readonly Mock<IRepository<ProcessStep>> _repoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private ProcessStepAppService CreateSut()
            => new ProcessStepAppService(_repoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var s1 = new ProcessStep(processId: 1, stepName: "Análise", stepOrder: 1, assignedRoleId: 1);
            var s2 = new ProcessStep(processId: 1, stepName: "Aprovação", stepOrder: 2, assignedRoleId: 1);
            TestHelpers.SetId(s1, 10);
            TestHelpers.SetId(s2, 20);

            _repoMock.Setup(r => r.GetAllAsync())
                     .ReturnsAsync(new[] { s1, s2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.StepId == 10);
            Assert.Contains(list, d => d.StepId == 20);

            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _repoMock.Setup(r => r.GetByIdAsync(99))
                     .ReturnsAsync((ProcessStep)null!);

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
            var step = new ProcessStep(processId: 2, stepName: "Revisão Técnica", stepOrder: 1, assignedRoleId: null);
            TestHelpers.SetId(step, 30);

            _repoMock.Setup(r => r.GetByIdAsync(30))
                     .ReturnsAsync(step);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(30);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(30, dto!.StepId);
            _repoMock.Verify(r => r.GetByIdAsync(30), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertAndCommit_AndReturnGeneratedId()
        {
            // Arrange
            var dto = new ProcessStepDto { StepId = 0, StepOrder = 1, 
                StepName = "Validação Final", AssignedRoleId = 1, ProcessId = 1 };

            _repoMock.Setup(r => r.InsertAsync(It.IsAny<ProcessStep>()))
                     .Callback<ProcessStep>(e => TestHelpers.SetId(e, 77))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var id = await sut.InsertAsync(dto);

            // Assert
            Assert.Equal(77, id);
            _repoMock.Verify(r => r.InsertAsync(It.IsAny<ProcessStep>()), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenStepNotFound()
        {
            // Arrange
            var dto = new ProcessStepDto { StepId = 77, ProcessId = 1, 
                StepName = "Atualizar Etapa", AssignedRoleId = 1, StepOrder = 1 };
            _repoMock.Setup(r => r.GetByIdAsync(77)).ReturnsAsync((ProcessStep)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Step not found.", ex.Message);

            _repoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ProcessStep>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new ProcessStep(processId: 3, stepName: "Análise Preliminar", 
                stepOrder: 1, assignedRoleId: null);
            TestHelpers.SetId(entity, 50);
            var dto = new ProcessStepDto { StepId = 50, ProcessId = 3, 
                StepName = "Análise Revisada", StepOrder = 1, AssignedRoleId = 1 };

            _repoMock.Setup(r => r.GetByIdAsync(50))
                     .ReturnsAsync(entity);

            _repoMock.Setup(r => r.UpdateAsync(entity))
                     .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _repoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _repoMock.Verify(r => r.UpdateAsync(It.Is<ProcessStep>(e => e == entity)), Times.Once);
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

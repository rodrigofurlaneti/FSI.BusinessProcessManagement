using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Application.Tests;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class ProcessExecutionAppServiceTests
    {
        private readonly Mock<IRepository<ProcessExecution>> _execRepoMock = new();
        private readonly Mock<IRepository<Process>> _procRepoMock = new();
        private readonly Mock<IUnitOfWork> _uowMock = new();

        private ProcessExecutionAppService CreateSut()
            => new ProcessExecutionAppService(_execRepoMock.Object, _procRepoMock.Object, _uowMock.Object);

        [Fact]
        public async Task GetAllAsync_ShouldReturnDtos_FromRepositoryEntities()
        {
            // Arrange
            var e1 = new ProcessExecution(processId: 1, stepId: 10, userId: 100);
            var e2 = new ProcessExecution(processId: 2, stepId: 20, userId: 200);
            TestHelpers.SetId(e1, 1);
            TestHelpers.SetId(e2, 2);

            _execRepoMock.Setup(r => r.GetAllAsync())
                         .ReturnsAsync(new[] { e1, e2 }.AsEnumerable());

            var sut = CreateSut();

            // Act
            var result = await sut.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, d => d.ExecutionId == 1);
            Assert.Contains(list, d => d.ExecutionId == 2);

            _execRepoMock.Verify(r => r.GetAllAsync(), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            _execRepoMock.Setup(r => r.GetByIdAsync(99))
                         .ReturnsAsync((ProcessExecution)null!);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(99);

            // Assert
            Assert.Null(dto);
            _execRepoMock.Verify(r => r.GetByIdAsync(99), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDto_WhenFound()
        {
            // Arrange
            var entity = new ProcessExecution(processId: 1, stepId: 2, userId: 3);
            TestHelpers.SetId(entity, 10);

            _execRepoMock.Setup(r => r.GetByIdAsync(10))
                         .ReturnsAsync(entity);

            var sut = CreateSut();

            // Act
            var dto = await sut.GetByIdAsync(10);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(10, dto!.ExecutionId);

            _execRepoMock.Verify(r => r.GetByIdAsync(10), Times.Once);
            _uowMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task InsertAsync_ShouldThrow_WhenProcessNotFound()
        {
            // Arrange
            var dto = new ProcessExecutionDto { ProcessId = 999, StepId = 1, UserId = 2 };

            _procRepoMock.Setup(r => r.GetByIdAsync(999))
                         .ReturnsAsync((Process)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.InsertAsync(dto));
            Assert.Equal("Process not found.", ex.Message);

            _procRepoMock.Verify(r => r.GetByIdAsync(999), Times.Once);
            _execRepoMock.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenExecutionNotFound()
        {
            // Arrange
            var dto = new ProcessExecutionDto { ExecutionId = 77, ProcessId = 1, StepId = 2, UserId = 3 };

            _execRepoMock.Setup(r => r.GetByIdAsync(77))
                         .ReturnsAsync((ProcessExecution)null!);

            var sut = CreateSut();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => sut.UpdateAsync(dto));
            Assert.Equal("Execution not found.", ex.Message);

            _execRepoMock.Verify(r => r.GetByIdAsync(77), Times.Once);
            _execRepoMock.Verify(r => r.UpdateAsync(It.IsAny<ProcessExecution>()), Times.Never);
            _uowMock.Verify(u => u.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAndCommit_WhenEntityFound()
        {
            // Arrange
            var entity = new ProcessExecution(processId: 1, stepId: 2, userId: 3);
            TestHelpers.SetId(entity, 50);

            var dto = new ProcessExecutionDto { ExecutionId = 50, ProcessId = 1, StepId = 99, UserId = 100 };

            _execRepoMock.Setup(r => r.GetByIdAsync(50))
                         .ReturnsAsync(entity);

            _execRepoMock.Setup(r => r.UpdateAsync(entity))
                         .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.UpdateAsync(dto);

            // Assert
            _execRepoMock.Verify(r => r.GetByIdAsync(50), Times.Once);
            _execRepoMock.Verify(r => r.UpdateAsync(It.Is<ProcessExecution>(e => e == entity)), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteAndCommit()
        {
            // Arrange
            _execRepoMock.Setup(r => r.DeleteAsync(9)).Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.DeleteAsync(9);

            // Assert
            _execRepoMock.Verify(r => r.DeleteAsync(9), Times.Once);
            _uowMock.Verify(u => u.CommitAsync(), Times.Once);
        }
    }
}
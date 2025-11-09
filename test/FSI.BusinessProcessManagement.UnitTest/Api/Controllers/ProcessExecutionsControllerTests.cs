using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Api.Controllers;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Controllers
{
    public sealed class ProcessExecutionsControllerTests
    {
        private readonly Mock<IProcessExecutionAppService> _serviceMock;
        private readonly ProcessExecutionsController _controller;

        public ProcessExecutionsControllerTests()
        {
            _serviceMock = new Mock<IProcessExecutionAppService>(MockBehavior.Strict);
            _controller = new ProcessExecutionsController(_serviceMock.Object);
        }

        private static ProcessExecutionDto MakeDto(
            long id = 1,
            long processId = 10,
            long stepId = 100,
            long? userId = 1000,
            string status = "Pendente",
            DateTime? started = null,
            DateTime? completed = null,
            string? remarks = "ok"
        ) => new ProcessExecutionDto
        {
            ExecutionId = id,
            ProcessId = processId,
            StepId = stepId,
            UserId = userId,
            Status = status,
            StartedAt = started,
            CompletedAt = completed,
            Remarks = remarks
        };

        // -------------------------------
        // GET /api/ProcessExecutions
        // -------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var data = new List<ProcessExecutionDto>
            {
                MakeDto(1, processId: 10, stepId: 100),
                MakeDto(2, processId: 11, stepId: 101, status: "Em Andamento")
            };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<ProcessExecutionDto>>(ok.Value);
            using var enumerator = payload.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.Equal(1, enumerator.Current.ExecutionId);
            Assert.True(enumerator.MoveNext());
            Assert.Equal(2, enumerator.Current.ExecutionId);

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAll_ShouldPropagateException_WhenServiceThrows()
        {
            _serviceMock.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException("boom"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAll());
            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // GET /api/ProcessExecutions/{id}
        // -------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            var dto = MakeDto(10, processId: 20, stepId: 200, status: "Concluído");
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            var result = await _controller.GetById(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<ProcessExecutionDto>(ok.Value);
            Assert.Equal(10, payload.ExecutionId);
            Assert.Equal("Concluído", payload.Status);

            _serviceMock.Verify(s => s.GetByIdAsync(10), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProcessExecutionDto?)null);

            var result = await _controller.GetById(999);

            Assert.IsType<NotFoundResult>(result.Result);
            _serviceMock.Verify(s => s.GetByIdAsync(999), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldPropagateException_WhenServiceThrows()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(7)).ThrowsAsync(new Exception("err"));

            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(7));
            _serviceMock.Verify(s => s.GetByIdAsync(7), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // POST /api/ProcessExecutions
        // -------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            var input = new ProcessExecutionDto { ProcessId = 50, StepId = 500, UserId = 5, Status = "Pendente" };
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<ProcessExecutionDto>())).ReturnsAsync(123L);

            var result = await _controller.Create(input);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProcessExecutionsController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.Is<ProcessExecutionDto>(d =>
                d.ProcessId == 50 &&
                d.StepId == 500 &&
                d.UserId == 5 &&
                d.Status == "Pendente"
            )), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            var input = MakeDto(0, processId: 60, stepId: 600, status: "Em Andamento");
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<ProcessExecutionDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<ProcessExecutionDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // PUT /api/ProcessExecutions/{id}
        // -------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            var input = new ProcessExecutionDto { ExecutionId = 0, ProcessId = 70, StepId = 700, Status = "Em Andamento", Remarks = "andando" };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ProcessExecutionDto>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(77, input);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<ProcessExecutionDto>(d =>
                d.ExecutionId == 77 &&
                d.ProcessId == 70 &&
                d.StepId == 700 &&
                d.Status == "Em Andamento" &&
                d.Remarks == "andando"
            )), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            var input = MakeDto(0, processId: 80, stepId: 800, status: "Pendente");
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ProcessExecutionDto>()))
                        .ThrowsAsync(new Exception("boom"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<ProcessExecutionDto>(d => d.ExecutionId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // DELETE /api/ProcessExecutions/{id}
        // -------------------------------
        [Fact]
        public async Task Delete_ShouldCallService_AndReturnNoContent()
        {
            _serviceMock.Setup(s => s.DeleteAsync(55)).Returns(Task.CompletedTask);

            var result = await _controller.Delete(55);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeleteAsync(55), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Delete_ShouldPropagateException_WhenServiceThrows()
        {
            _serviceMock.Setup(s => s.DeleteAsync(88)).ThrowsAsync(new InvalidOperationException("x"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete(88));
            _serviceMock.Verify(s => s.DeleteAsync(88), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }
    }
}

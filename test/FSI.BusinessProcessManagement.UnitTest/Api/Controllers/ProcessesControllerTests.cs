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
    public sealed class ProcessesControllerTests
    {
        private readonly Mock<IProcessoBPMAppService> _serviceMock;
        private readonly ProcessesController _controller;

        public ProcessesControllerTests()
        {
            _serviceMock = new Mock<IProcessoBPMAppService>(MockBehavior.Strict);
            _controller = new ProcessesController(_serviceMock.Object);
        }

        private static ProcessoBPMDto MakeDto(
            long id = 1,
            string name = "Onboarding",
            long? deptId = 10,
            string? desc = "Processo de onboarding",
            long? createdBy = 1001
            ) => new ProcessoBPMDto
            {
                ProcessId = id,
                ProcessName = name,
                DepartmentId = deptId,
                Description = desc,
                CreatedBy = createdBy
            };

        // -------------------------------
        // GET /api/Processes
        // -------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var data = new List<ProcessoBPMDto> { MakeDto(1), MakeDto(2, "Offboarding") };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<ProcessoBPMDto>>(ok.Value);
            Assert.Collection(payload,
                first => Assert.Equal(1, first.ProcessId),
                second => Assert.Equal(2, second.ProcessId));

            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAll_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetAllAsync()).ThrowsAsync(new InvalidOperationException("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAll());
            _serviceMock.Verify(s => s.GetAllAsync(), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // GET /api/Processes/{id}
        // -------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var dto = MakeDto(10, "Aprovação de Compras");
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<ProcessoBPMDto>(ok.Value);
            Assert.Equal(10, payload.ProcessId);
            Assert.Equal("Aprovação de Compras", payload.ProcessName);

            _serviceMock.Verify(s => s.GetByIdAsync(10), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((ProcessoBPMDto?)null);

            // Act
            var result = await _controller.GetById(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _serviceMock.Verify(s => s.GetByIdAsync(999), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(7)).ThrowsAsync(new Exception("err"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.GetById(7));
            _serviceMock.Verify(s => s.GetByIdAsync(7), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // POST /api/Processes
        // -------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            // Arrange
            var input = new ProcessoBPMDto { ProcessName = "Financeiro - Pagamentos", DepartmentId = 50, Description = "Fluxo de pagamentos" };
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<ProcessoBPMDto>())).ReturnsAsync(123L);

            // Act
            var result = await _controller.Create(input);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(ProcessesController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.Is<ProcessoBPMDto>(d =>
                d.ProcessName == "Financeiro - Pagamentos" &&
                d.DepartmentId == 50 &&
                d.Description == "Fluxo de pagamentos")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Qualidade - Auditorias");
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<ProcessoBPMDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<ProcessoBPMDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // PUT /api/Processes/{id}
        // -------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            // Arrange
            var input = new ProcessoBPMDto { ProcessId = 0, ProcessName = "Comercial - Propostas", DepartmentId = 70, Description = "Criação de propostas" };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ProcessoBPMDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(77, input);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<ProcessoBPMDto>(d =>
                d.ProcessId == 77 &&
                d.ProcessName == "Comercial - Propostas" &&
                d.DepartmentId == 70 &&
                d.Description == "Criação de propostas")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Jurídico - Contratos");
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<ProcessoBPMDto>()))
                        .ThrowsAsync(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<ProcessoBPMDto>(d => d.ProcessId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // DELETE /api/Processes/{id}
        // -------------------------------
        [Fact]
        public async Task Delete_ShouldCallService_AndReturnNoContent()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(55)).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Delete(55);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.DeleteAsync(55), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Delete_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            _serviceMock.Setup(s => s.DeleteAsync(88)).ThrowsAsync(new InvalidOperationException("x"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Delete(88));
            _serviceMock.Verify(s => s.DeleteAsync(88), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }
    }
}

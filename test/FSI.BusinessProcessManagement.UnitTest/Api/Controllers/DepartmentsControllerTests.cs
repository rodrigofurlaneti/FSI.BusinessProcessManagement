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
    public sealed class DepartmentsControllerTests
    {
        private readonly Mock<IDepartmentAppService> _serviceMock;
        private readonly DepartmentsController _controller;

        public DepartmentsControllerTests()
        {
            _serviceMock = new Mock<IDepartmentAppService>(MockBehavior.Strict);
            _controller = new DepartmentsController(_serviceMock.Object);
        }

        private static DepartmentDto MakeDto(long id = 1, string name = "TI") => new DepartmentDto
        {
            DepartmentId = id,
            DepartmentName = name,
            Description = "Depto de Tecnologia"
        };

        // -------------------------------
        // GET /api/Departments
        // -------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var data = new List<DepartmentDto> { MakeDto(1), MakeDto(2, "RH") };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<DepartmentDto>>(ok.Value);
            Assert.Collection(payload,
                first => Assert.Equal(1, first.DepartmentId),
                second => Assert.Equal(2, second.DepartmentId));

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
        // GET /api/Departments/{id}
        // -------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var dto = MakeDto(10, "Operações");
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<DepartmentDto>(ok.Value);
            Assert.Equal(10, payload.DepartmentId);
            Assert.Equal("Operações", payload.DepartmentName);

            _serviceMock.Verify(s => s.GetByIdAsync(10), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((DepartmentDto?)null);

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
        // POST /api/Departments
        // -------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            // Arrange
            var input = new DepartmentDto { DepartmentName = "Financeiro", Description = "Pagamentos e recebimentos" };
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<DepartmentDto>())).ReturnsAsync(123L);

            // Act
            var result = await _controller.Create(input);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(DepartmentsController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.Is<DepartmentDto>(d =>
                d.DepartmentName == "Financeiro" &&
                d.Description == "Pagamentos e recebimentos")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Qualidade");
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<DepartmentDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<DepartmentDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // PUT /api/Departments/{id}
        // -------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            // Arrange
            var input = new DepartmentDto { DepartmentId = 0, DepartmentName = "Comercial", Description = "Vendas" };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<DepartmentDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(77, input);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<DepartmentDto>(d =>
                d.DepartmentId == 77 &&
                d.DepartmentName == "Comercial" &&
                d.Description == "Vendas")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Jurídico");
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<DepartmentDto>()))
                        .ThrowsAsync(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<DepartmentDto>(d => d.DepartmentId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // DELETE /api/Departments/{id}
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

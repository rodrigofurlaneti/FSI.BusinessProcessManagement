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
    public sealed class RolesControllerTests
    {
        private readonly Mock<IRoleAppService> _serviceMock;
        private readonly RolesController _controller;

        public RolesControllerTests()
        {
            _serviceMock = new Mock<IRoleAppService>(MockBehavior.Strict);
            _controller = new RolesController(_serviceMock.Object);
        }

        private static RoleDto MakeDto(long id = 1, string? name = "Admin", string? desc = "Administrador")
            => new RoleDto { RoleId = id, RoleName = name, Description = desc };

        // -------------------------------
        // GET /api/Roles
        // -------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var data = new List<RoleDto> { MakeDto(1), MakeDto(2, "Viewer", "Leitura") };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<RoleDto>>(ok.Value);
            Assert.Collection(payload,
                first => Assert.Equal(1, first.RoleId),
                second => Assert.Equal(2, second.RoleId));

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
        // GET /api/Roles/{id}
        // -------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var dto = MakeDto(10, "Editor", "Edição");
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(10);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<RoleDto>(ok.Value);
            Assert.Equal(10, payload.RoleId);
            Assert.Equal("Editor", payload.RoleName);

            _serviceMock.Verify(s => s.GetByIdAsync(10), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((RoleDto?)null);

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
        // POST /api/Roles
        // -------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            // Arrange
            var input = new RoleDto { RoleName = "Financeiro", Description = "Permissões de finanças" };
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<RoleDto>())).ReturnsAsync(123L);

            // Act
            var result = await _controller.Create(input);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(RolesController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.Is<RoleDto>(d =>
                d.RoleName == "Financeiro" &&
                d.Description == "Permissões de finanças")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Qualidade", "Auditorias");
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<RoleDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<RoleDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // PUT /api/Roles/{id}
        // -------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            // Arrange
            var input = new RoleDto { RoleId = 0, RoleName = "Comercial", Description = "Vendas" };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<RoleDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(77, input);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<RoleDto>(d =>
                d.RoleId == 77 &&
                d.RoleName == "Comercial" &&
                d.Description == "Vendas")), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0, "Jurídico", "Contratos");
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<RoleDto>()))
                        .ThrowsAsync(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<RoleDto>(d => d.RoleId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // DELETE /api/Roles/{id}
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

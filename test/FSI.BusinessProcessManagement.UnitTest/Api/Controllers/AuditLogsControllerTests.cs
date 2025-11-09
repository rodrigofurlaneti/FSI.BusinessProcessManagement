using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Api.Controllers;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Controllers
{
    public sealed class AuditLogsControllerTests
    {
        private readonly Mock<IAuditLogAppService> _serviceMock;
        private readonly AuditLogsController _controller;

        public AuditLogsControllerTests()
        {
            _serviceMock = new Mock<IAuditLogAppService>(MockBehavior.Strict);
            _controller = new AuditLogsController(_serviceMock.Object);
        }

        private static AuditLogDto MakeDto(long id = 1) => new AuditLogDto
        {
            AuditId = id,
            UserId = 10,
            ScreenId = 20,
            ActionType = "CREATE",
            ActionTimestamp = new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            AdditionalInfo = "ok"
        };

        // -------------------------------
        // GET /api/AuditLogs
        // -------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            // Arrange
            var data = new List<AuditLogDto> { MakeDto(1), MakeDto(2) };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            // Act
            var result = await _controller.GetAll();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<AuditLogDto>>(ok.Value);
            Assert.Collection(payload,
                item => Assert.Equal(1, item.AuditId),
                item => Assert.Equal(2, item.AuditId));

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
        // GET /api/AuditLogs/{id}
        // -------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var dto = MakeDto(42);
            _serviceMock.Setup(s => s.GetByIdAsync(42)).ReturnsAsync(dto);

            // Act
            var result = await _controller.GetById(42);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<AuditLogDto>(ok.Value);
            Assert.Equal(42, payload.AuditId);

            _serviceMock.Verify(s => s.GetByIdAsync(42), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            // Arrange
            _serviceMock.Setup(s => s.GetByIdAsync(99)).ReturnsAsync((AuditLogDto?)null);

            // Act
            var result = await _controller.GetById(99);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _serviceMock.Verify(s => s.GetByIdAsync(99), Times.Once);
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
        // POST /api/AuditLogs
        // -------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            // Arrange
            var input = MakeDto(0);
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<AuditLogDto>())).ReturnsAsync(123L);

            // Act
            var result = await _controller.Create(input);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(AuditLogsController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.Is<AuditLogDto>(d =>
                d.ActionType == input.ActionType &&
                d.UserId == input.UserId &&
                d.ScreenId == input.ScreenId &&
                d.AdditionalInfo == input.AdditionalInfo
            )), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0);
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<AuditLogDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<AuditLogDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // PUT /api/AuditLogs/{id}
        // -------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            // Arrange
            var input = MakeDto(0); // chegará sem id
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AuditLogDto>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Update(77, input);

            // Assert
            Assert.IsType<NoContentResult>(result);

            _serviceMock.Verify(s => s.UpdateAsync(It.Is<AuditLogDto>(d =>
                d.AuditId == 77 &&                     // controller deve forçar o id da rota
                d.ActionType == input.ActionType &&
                d.UserId == input.UserId &&
                d.ScreenId == input.ScreenId &&
                d.AdditionalInfo == input.AdditionalInfo
            )), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            // Arrange
            var input = MakeDto(0);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<AuditLogDto>()))
                        .ThrowsAsync(new Exception("boom"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<AuditLogDto>(d => d.AuditId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // -------------------------------
        // DELETE /api/AuditLogs/{id}
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

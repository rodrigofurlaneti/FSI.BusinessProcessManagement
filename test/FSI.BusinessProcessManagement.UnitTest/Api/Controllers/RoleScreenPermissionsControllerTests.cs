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
    public sealed class RoleScreenPermissionsControllerTests
    {
        private readonly Mock<IRoleScreenPermissionAppService> _serviceMock;
        private readonly RoleScreenPermissionsController _controller;

        public RoleScreenPermissionsControllerTests()
        {
            _serviceMock = new Mock<IRoleScreenPermissionAppService>(MockBehavior.Strict);
            _controller = new RoleScreenPermissionsController(_serviceMock.Object);
        }

        private static RoleScreenPermissionDto MakeDto(long id = 1)
            => new RoleScreenPermissionDto { RoleScreenPermissionId = id };

        // ---------------------------------------
        // GET /api/RoleScreenPermissions
        // ---------------------------------------
        [Fact]
        public async Task GetAll_ShouldReturnOk_WithList()
        {
            var data = new List<RoleScreenPermissionDto> { MakeDto(1), MakeDto(2) };
            _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(data);

            var result = await _controller.GetAll();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsAssignableFrom<IEnumerable<RoleScreenPermissionDto>>(ok.Value);
            using var it = payload.GetEnumerator();
            Assert.True(it.MoveNext());
            Assert.Equal(1, it.Current.RoleScreenPermissionId);
            Assert.True(it.MoveNext());
            Assert.Equal(2, it.Current.RoleScreenPermissionId);

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

        // ---------------------------------------
        // GET /api/RoleScreenPermissions/{id}
        // ---------------------------------------
        [Fact]
        public async Task GetById_ShouldReturnOk_WhenFound()
        {
            var dto = MakeDto(10);
            _serviceMock.Setup(s => s.GetByIdAsync(10)).ReturnsAsync(dto);

            var result = await _controller.GetById(10);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<RoleScreenPermissionDto>(ok.Value);
            Assert.Equal(10, payload.RoleScreenPermissionId);

            _serviceMock.Verify(s => s.GetByIdAsync(10), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenNull()
        {
            _serviceMock.Setup(s => s.GetByIdAsync(999)).ReturnsAsync((RoleScreenPermissionDto?)null);

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

        // ---------------------------------------
        // POST /api/RoleScreenPermissions
        // ---------------------------------------
        [Fact]
        public async Task Create_ShouldReturnCreatedAt_WithNewId()
        {
            var input = new RoleScreenPermissionDto();
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<RoleScreenPermissionDto>())).ReturnsAsync(123L);

            var result = await _controller.Create(input);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(nameof(RoleScreenPermissionsController.GetById), created.ActionName);
            Assert.NotNull(created.RouteValues);
            Assert.True(created.RouteValues!.ContainsKey("id"));
            Assert.Equal(123L, created.RouteValues["id"]);
            Assert.Equal(123L, created.Value);

            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<RoleScreenPermissionDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Create_ShouldPropagateException_WhenServiceThrows()
        {
            var input = MakeDto(0);
            _serviceMock.Setup(s => s.InsertAsync(It.IsAny<RoleScreenPermissionDto>()))
                        .ThrowsAsync(new InvalidOperationException("fail"));

            await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Create(input));
            _serviceMock.Verify(s => s.InsertAsync(It.IsAny<RoleScreenPermissionDto>()), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------------------
        // PUT /api/RoleScreenPermissions/{id}
        // ---------------------------------------
        [Fact]
        public async Task Update_ShouldSetDtoId_CallService_AndReturnNoContent()
        {
            var input = new RoleScreenPermissionDto { RoleScreenPermissionId = 0 };
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<RoleScreenPermissionDto>())).Returns(Task.CompletedTask);

            var result = await _controller.Update(77, input);

            Assert.IsType<NoContentResult>(result);
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<RoleScreenPermissionDto>(d => d.RoleScreenPermissionId == 77)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Update_ShouldPropagateException_WhenServiceThrows()
        {
            var input = MakeDto(0);
            _serviceMock.Setup(s => s.UpdateAsync(It.IsAny<RoleScreenPermissionDto>()))
                        .ThrowsAsync(new Exception("boom"));

            await Assert.ThrowsAsync<Exception>(() => _controller.Update(5, input));
            _serviceMock.Verify(s => s.UpdateAsync(It.Is<RoleScreenPermissionDto>(d => d.RoleScreenPermissionId == 5)), Times.Once);
            _serviceMock.VerifyNoOtherCalls();
        }

        // ---------------------------------------
        // DELETE /api/RoleScreenPermissions/{id}
        // ---------------------------------------
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

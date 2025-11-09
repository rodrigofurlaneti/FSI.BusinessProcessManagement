using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Api.Controllers;
using FSI.BusinessProcessManagement.Api.Models.Auth;
using FSI.BusinessProcessManagement.Api.Services;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Helper = FSI.BusinessProcessManagement.UnitTests.Api.Helpers.Api;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Controllers
{
    public sealed class AuthControllerTests
    {
        private readonly Mock<IRepository<User>> _users;
        private readonly Mock<IRepository<UserRole>> _userRoles;
        private readonly Mock<IRepository<Role>> _roles;
        private readonly Mock<ITokenService> _tokenService;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _users = new Mock<IRepository<User>>(MockBehavior.Strict);
            _userRoles = new Mock<IRepository<UserRole>>(MockBehavior.Strict);
            _roles = new Mock<IRepository<Role>>(MockBehavior.Strict);
            _tokenService = new Mock<ITokenService>(MockBehavior.Strict);

            _controller = new AuthController(
                _users.Object,
                _userRoles.Object,
                _roles.Object,
                _tokenService.Object
            );
        }

        [Theory]
        [InlineData("", "x")]
        [InlineData("   ", "x")]
        [InlineData("u", "")]
        [InlineData("u", "   ")]
        public async Task Login_ShouldReturnBadRequest_WhenUsernameOrPasswordMissing(string username, string password)
        {
            var req = new LoginRequest { Username = username, Password = password };

            var result = await _controller.Login(req);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Username and password are required.", bad.Value);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserNotFound()
        {
            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<User>());

            var req = new LoginRequest { Username = "not-exists", Password = "x" };
            var result = await _controller.Login(req);

            Assert.IsType<UnauthorizedResult>(result.Result);
            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenUserInactive()
        {
            var list = new List<User> { Helper.MakeUser(1, "rodrigo", "secret", isActive: false) };
            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(list);

            var req = new LoginRequest { Username = "rodrigo", Password = "whatever" };
            var result = await _controller.Login(req);

            Assert.IsType<UnauthorizedResult>(result.Result);
            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordMismatch_PlainText()
        {
            var user = Helper.MakeUser(10, "ana", "secret"); // stored plain
            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });

            var req = new LoginRequest { Username = "ana", Password = "WRONG" };
            var result = await _controller.Login(req);

            Assert.IsType<UnauthorizedResult>(result.Result);
            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnUnauthorized_WhenPasswordMismatch_Bcrypt()
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!");
            var user = Helper.MakeUser(11, "ana", hashed);
            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });

            var req = new LoginRequest { Username = "ana", Password = "diff" };
            var result = await _controller.Login(req);

            Assert.IsType<UnauthorizedResult>(result.Result);
            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithTokenAndRoles_PlainText()
        {
            var user = Helper.MakeUser(20, "ana", "123"); // plain
            var userRoles = new[] { Helper.MakeUserRole(20, 1), Helper.MakeUserRole(20, 2) };
            var roles = new[] { Helper.MakeRole(1, "Admin"), Helper.MakeRole(2, "Viewer"), Helper.MakeRole(3, "Ignored") };

            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });
            _userRoles.Setup(r => r.GetAllAsync()).ReturnsAsync(userRoles);
            _roles.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);

            var expectedToken = "jwt-token";
            var expectedExpiry = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            IEnumerable<Claim>? capturedClaims = null;
            _tokenService
                .Setup(t => t.GenerateTokenWithExpiry(It.IsAny<IEnumerable<Claim>>(), It.IsAny<DateTime?>()))
                .Callback<IEnumerable<Claim>, DateTime?>((cs, _) => capturedClaims = cs)
                .Returns((expectedToken, expectedExpiry));

            var req = new LoginRequest { Username = "ana", Password = "123" };
            var result = await _controller.Login(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<LoginResponse>(ok.Value);

            Assert.Equal(expectedToken, payload.AccessToken);
            Assert.Equal("Bearer", payload.TokenType);
            Assert.Equal(expectedExpiry, payload.ExpiresAtUtc);
            Assert.Equal(20, payload.UserId);
            Assert.Equal("ana", payload.Username);
            Assert.True(payload.Roles.SequenceEqual(new[] { "Admin", "Viewer" }));

            _tokenService.Verify(t => t.GenerateTokenWithExpiry(It.IsAny<IEnumerable<Claim>>(), It.IsAny<DateTime?>()), Times.Once);

            Assert.NotNull(capturedClaims);
            Assert.Contains(capturedClaims!, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "20");
            Assert.Contains(capturedClaims!, c => c.Type == ClaimTypes.Name && c.Value == "ana");
            Assert.Equal(2, capturedClaims!.Count(c => c.Type == ClaimTypes.Role));

            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _userRoles.Verify(r => r.GetAllAsync(), Times.Once);
            _roles.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Login_ShouldReturnOk_WithTokenAndRoles_Bcrypt_DistinctAndNoEmpty()
        {
            var hashed = BCrypt.Net.BCrypt.HashPassword("Strong#123");
            var user = Helper.MakeUser(30, "joao", hashed);

            var userRoles = new[]
            {
        Helper.MakeUserRole(30, 1),
        Helper.MakeUserRole(30, 2),
        Helper.MakeUserRole(30, 2), 
        };
                var roles = new[]
                {
                    Helper.MakeRole(1, "Editor"),
                    Helper.MakeRole(2, "Editor"),
                };

            _users.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { user });
            _userRoles.Setup(r => r.GetAllAsync()).ReturnsAsync(userRoles);
            _roles.Setup(r => r.GetAllAsync()).ReturnsAsync(roles);

            var token = "jwt2";
            var exp = DateTime.UtcNow.AddHours(1);

            IEnumerable<Claim>? capturedClaims = null;
            _tokenService
                .Setup(t => t.GenerateTokenWithExpiry(It.IsAny<IEnumerable<Claim>>(), It.IsAny<DateTime?>()))
                .Callback<IEnumerable<Claim>, DateTime?>((cs, _) => capturedClaims = cs)
                .Returns((token, exp));

            var req = new LoginRequest { Username = "joao", Password = "Strong#123" };
            var result = await _controller.Login(req);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var payload = Assert.IsType<LoginResponse>(ok.Value);

            Assert.Equal("joao", payload.Username);
            Assert.Equal(30, payload.UserId);
            Assert.True(payload.Roles.SequenceEqual(new[] { "Editor" }));

            _tokenService.Verify(t => t.GenerateTokenWithExpiry(It.IsAny<IEnumerable<Claim>>(), It.IsAny<DateTime?>()), Times.Once);

            Assert.NotNull(capturedClaims);
            var roleClaims = capturedClaims!.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            Assert.Single(roleClaims);
            Assert.Equal("Editor", roleClaims[0]);

            _users.Verify(r => r.GetAllAsync(), Times.Once);
            _userRoles.Verify(r => r.GetAllAsync(), Times.Once);
            _roles.Verify(r => r.GetAllAsync(), Times.Once);
            _users.VerifyNoOtherCalls();
            _userRoles.VerifyNoOtherCalls();
            _roles.VerifyNoOtherCalls();
            _tokenService.VerifyNoOtherCalls();
        }

    }
}
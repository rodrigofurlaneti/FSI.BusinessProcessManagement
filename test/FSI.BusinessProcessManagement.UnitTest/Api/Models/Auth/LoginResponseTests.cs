using System;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Api.Models.Auth;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Models.Auth
{
    public sealed class LoginResponseTests
    {
        [Fact]
        public void DefaultValues_ShouldBeInitializedCorrectly()
        {
            // Arrange & Act
            var resp = new LoginResponse();

            // Assert
            Assert.Equal(string.Empty, resp.AccessToken);
            Assert.Equal("Bearer", resp.TokenType);
            Assert.Equal(default(DateTime), resp.ExpiresAtUtc);
            Assert.Equal(0L, resp.UserId);
            Assert.Equal(string.Empty, resp.Username);
            Assert.Empty(resp.Roles);
        }

        [Fact]
        public void ShouldAllow_SettingAndGetting_AllProperties()
        {
            // Arrange
            var roles = new List<string> { "Admin", "Viewer" };
            var exp = new DateTime(2030, 12, 25, 10, 0, 0, DateTimeKind.Utc);

            var resp = new LoginResponse
            {
                AccessToken = "jwt-token",
                TokenType = "Custom",
                ExpiresAtUtc = exp,
                UserId = 99,
                Username = "rodrigo",
                Roles = roles
            };

            // Assert
            Assert.Equal("jwt-token", resp.AccessToken);
            Assert.Equal("Custom", resp.TokenType);
            Assert.Equal(exp, resp.ExpiresAtUtc);
            Assert.Equal(99, resp.UserId);
            Assert.Equal("rodrigo", resp.Username);
            Assert.Same(roles, resp.Roles);
        }

        [Fact]
        public void ShouldHandle_EmptyRoles_WithoutThrowing()
        {
            // Arrange
            var resp = new LoginResponse { Roles = Array.Empty<string>() };

            // Act & Assert
            Assert.NotNull(resp.Roles);
            Assert.Empty(resp.Roles);
        }

        [Fact]
        public void ShouldAllow_NullAssignments_ForOptionalProperties()
        {
            // Arrange
            var resp = new LoginResponse
            {
                AccessToken = null,
                TokenType = null,
                Username = null,
                Roles = null
            };

            // Assert
            Assert.Null(resp.AccessToken);
            Assert.Null(resp.TokenType);
            Assert.Null(resp.Username);
            Assert.Null(resp.Roles);
        }
    }
}

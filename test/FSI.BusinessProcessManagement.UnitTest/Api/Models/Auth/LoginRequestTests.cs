using FSI.BusinessProcessManagement.Api.Models.Auth;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Api.Models.Auth
{
    public sealed class LoginRequestTests
    {
        [Fact]
        public void DefaultValues_ShouldBeEmptyStrings()
        {
            // Arrange & Act
            var req = new LoginRequest();

            // Assert
            Assert.Equal(string.Empty, req.Username);
            Assert.Equal(string.Empty, req.Password);
        }

        [Fact]
        public void ShouldAllow_SettingAndGetting_UsernameAndPassword()
        {
            // Arrange
            var req = new LoginRequest
            {
                Username = "rodrigo",
                Password = "123456"
            };

            // Act & Assert
            Assert.Equal("rodrigo", req.Username);
            Assert.Equal("123456", req.Password);
        }

        [Fact]
        public void ShouldAllow_NullAssignments_EvenIfNotRecommended()
        {
            // Arrange
            var req = new LoginRequest
            {
                Username = null,
                Password = null
            };

            // Act & Assert
            Assert.Null(req.Username);
            Assert.Null(req.Password);
        }
    }
}

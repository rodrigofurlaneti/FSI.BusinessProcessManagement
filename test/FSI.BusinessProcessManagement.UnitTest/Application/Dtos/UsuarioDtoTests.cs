using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class UsuarioDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new UsuarioDto();

            // Assert
            Assert.Equal(0, dto.UserId);
            Assert.Null(dto.DepartmentId);
            Assert.Equal(string.Empty, dto.Username);
            Assert.Null(dto.PasswordHash);
            Assert.Null(dto.Email);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new UsuarioDto
            {
                UserId = 1,
                DepartmentId = 10,
                Username = "rfurlaneti",
                PasswordHash = "hash123",
                Email = "rodrigo.furlaneti@empresa.com",
                IsActive = false
            };

            // Act & Assert
            Assert.Equal(1, dto.UserId);
            Assert.Equal(10, dto.DepartmentId);
            Assert.Equal("rfurlaneti", dto.Username);
            Assert.Equal("hash123", dto.PasswordHash);
            Assert.Equal("rodrigo.furlaneti@empresa.com", dto.Email);
            Assert.False(dto.IsActive);
        }

        [Fact]
        public void Username_ShouldDefaultToEmptyString_WhenNotSet()
        {
            // Arrange
            var dto = new UsuarioDto();

            // Assert
            Assert.Equal(string.Empty, dto.Username);
        }

        [Fact]
        public void IsActive_ShouldDefaultToTrue_WhenNotSet()
        {
            // Arrange
            var dto = new UsuarioDto();

            // Assert
            Assert.True(dto.IsActive);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new UsuarioDto
            {
                DepartmentId = null,
                PasswordHash = null,
                Email = null
            };

            // Assert
            Assert.Null(dto.DepartmentId);
            Assert.Null(dto.PasswordHash);
            Assert.Null(dto.Email);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new UsuarioDto
            {
                Username = "antigo",
                IsActive = true
            };

            // Act
            dto.Username = "novo";
            dto.IsActive = false;

            // Assert
            Assert.Equal("novo", dto.Username);
            Assert.False(dto.IsActive);
        }
    }
}

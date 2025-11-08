using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class RoleDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new RoleDto();

            // Assert
            Assert.Equal(0, dto.RoleId);
            Assert.Equal(string.Empty, dto.RoleName);
            Assert.Null(dto.Description);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new RoleDto
            {
                RoleId = 10,
                RoleName = "Administrador",
                Description = "Acesso total ao sistema"
            };

            // Act & Assert
            Assert.Equal(10, dto.RoleId);
            Assert.Equal("Administrador", dto.RoleName);
            Assert.Equal("Acesso total ao sistema", dto.Description);
        }

        [Fact]
        public void RoleName_ShouldDefaultToEmptyString_WhenNotSet()
        {
            // Arrange
            var dto = new RoleDto();

            // Assert
            Assert.Equal(string.Empty, dto.RoleName);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new RoleDto
            {
                Description = null
            };

            // Assert
            Assert.Null(dto.Description);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new RoleDto
            {
                RoleName = "Usuário",
                Description = "Acesso restrito"
            };

            // Act
            dto.RoleName = "Gerente";
            dto.Description = "Acesso a relatórios e cadastros";

            // Assert
            Assert.Equal("Gerente", dto.RoleName);
            Assert.Equal("Acesso a relatórios e cadastros", dto.Description);
        }
    }
}

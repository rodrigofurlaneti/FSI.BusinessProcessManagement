using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class UserRoleDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new UserRoleDto();

            // Assert
            Assert.Equal(0, dto.UserRoleId);
            Assert.Equal(0, dto.UserId);
            Assert.Equal(0, dto.RoleId);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new UserRoleDto
            {
                UserRoleId = 10,
                UserId = 5,
                RoleId = 3
            };

            // Act & Assert
            Assert.Equal(10, dto.UserRoleId);
            Assert.Equal(5, dto.UserId);
            Assert.Equal(3, dto.RoleId);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new UserRoleDto
            {
                UserRoleId = 1,
                UserId = 2,
                RoleId = 3
            };

            // Act
            dto.UserId = 100;
            dto.RoleId = 200;

            // Assert
            Assert.Equal(100, dto.UserId);
            Assert.Equal(200, dto.RoleId);
        }
    }
}

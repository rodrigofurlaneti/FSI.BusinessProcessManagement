using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class RoleScreenPermissionDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new RoleScreenPermissionDto();

            // Assert
            Assert.Equal(0, dto.RoleScreenPermissionId);
            Assert.Equal(0, dto.RoleId);
            Assert.Equal(0, dto.ScreenId);
            Assert.False(dto.CanView);
            Assert.False(dto.CanCreate);
            Assert.False(dto.CanEdit);
            Assert.False(dto.CanDelete);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleScreenPermissionId = 1,
                RoleId = 10,
                ScreenId = 20,
                CanView = true,
                CanCreate = true,
                CanEdit = false,
                CanDelete = true
            };

            // Act & Assert
            Assert.Equal(1, dto.RoleScreenPermissionId);
            Assert.Equal(10, dto.RoleId);
            Assert.Equal(20, dto.ScreenId);
            Assert.True(dto.CanView);
            Assert.True(dto.CanCreate);
            Assert.False(dto.CanEdit);
            Assert.True(dto.CanDelete);
        }

        [Fact]
        public void BooleanProperties_ShouldBeFalse_ByDefault()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto();

            // Assert
            Assert.False(dto.CanView);
            Assert.False(dto.CanCreate);
            Assert.False(dto.CanEdit);
            Assert.False(dto.CanDelete);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleId = 1,
                ScreenId = 2,
                CanView = false,
                CanCreate = false,
                CanEdit = false,
                CanDelete = false
            };

            // Act
            dto.CanView = true;
            dto.CanCreate = true;
            dto.CanEdit = true;
            dto.CanDelete = true;

            // Assert
            Assert.True(dto.CanView);
            Assert.True(dto.CanCreate);
            Assert.True(dto.CanEdit);
            Assert.True(dto.CanDelete);
        }

        [Fact]
        public void NumericProperties_ShouldAcceptValidValues()
        {
            // Arrange
            var dto = new RoleScreenPermissionDto
            {
                RoleScreenPermissionId = 999,
                RoleId = 123,
                ScreenId = 456
            };

            // Assert
            Assert.Equal(999, dto.RoleScreenPermissionId);
            Assert.Equal(123, dto.RoleId);
            Assert.Equal(456, dto.ScreenId);
        }
    }
}

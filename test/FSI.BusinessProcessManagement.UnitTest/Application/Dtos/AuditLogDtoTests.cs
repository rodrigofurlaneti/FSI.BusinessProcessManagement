using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class AuditLogDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new AuditLogDto();

            // Assert
            Assert.Equal(0, dto.AuditId);
            Assert.Null(dto.UserId);
            Assert.Null(dto.ScreenId);
            Assert.Equal(string.Empty, dto.ActionType);
            Assert.Equal(default(DateTime), dto.ActionTimestamp);
            Assert.Null(dto.AdditionalInfo);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new AuditLogDto
            {
                AuditId = 123,
                UserId = 456,
                ScreenId = 789,
                ActionType = "CREATE",
                ActionTimestamp = new DateTime(2025, 11, 8, 10, 0, 0),
                AdditionalInfo = "User created a new process"
            };

            // Act & Assert
            Assert.Equal(123, dto.AuditId);
            Assert.Equal(456, dto.UserId);
            Assert.Equal(789, dto.ScreenId);
            Assert.Equal("CREATE", dto.ActionType);
            Assert.Equal(new DateTime(2025, 11, 8, 10, 0, 0), dto.ActionTimestamp);
            Assert.Equal("User created a new process", dto.AdditionalInfo);
        }
    }
}

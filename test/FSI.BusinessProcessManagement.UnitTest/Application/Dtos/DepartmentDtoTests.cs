using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class DepartmentDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new DepartmentDto();

            // Assert
            Assert.Equal(0, dto.DepartmentId);
            Assert.Null(dto.DepartmentName);
            Assert.Null(dto.Description);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new DepartmentDto
            {
                DepartmentId = 101,
                DepartmentName = "Tecnologia da Informação",
                Description = "Responsável pelos sistemas internos e infraestrutura"
            };

            // Act & Assert
            Assert.Equal(101, dto.DepartmentId);
            Assert.Equal("Tecnologia da Informação", dto.DepartmentName);
            Assert.Equal("Responsável pelos sistemas internos e infraestrutura", dto.Description);
        }

        [Fact]
        public void DepartmentName_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new DepartmentDto { DepartmentName = "TI" };

            // Act
            dto.DepartmentName = "Financeiro";

            // Assert
            Assert.Equal("Financeiro", dto.DepartmentName);
        }

        [Fact]
        public void Description_CanBeNull_WithoutThrowingException()
        {
            // Arrange
            var dto = new DepartmentDto
            {
                DepartmentId = 10,
                DepartmentName = "RH",
                Description = null
            };

            // Assert
            Assert.Null(dto.Description);
            Assert.Equal("RH", dto.DepartmentName);
        }
    }
}

using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class ProcessStepDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new ProcessStepDto();

            // Assert
            Assert.Equal(0, dto.StepId);
            Assert.Equal(0, dto.ProcessId);
            Assert.Null(dto.StepName);
            Assert.Equal(0, dto.StepOrder);
            Assert.Null(dto.AssignedRoleId);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new ProcessStepDto
            {
                StepId = 10,
                ProcessId = 5,
                StepName = "Validação de Documentos",
                StepOrder = 2,
                AssignedRoleId = 99
            };

            // Act & Assert
            Assert.Equal(10, dto.StepId);
            Assert.Equal(5, dto.ProcessId);
            Assert.Equal("Validação de Documentos", dto.StepName);
            Assert.Equal(2, dto.StepOrder);
            Assert.Equal(99, dto.AssignedRoleId);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new ProcessStepDto
            {
                StepName = null,
                AssignedRoleId = null
            };

            // Assert
            Assert.Null(dto.StepName);
            Assert.Null(dto.AssignedRoleId);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new ProcessStepDto
            {
                StepName = "Revisão Inicial",
                StepOrder = 1
            };

            // Act
            dto.StepName = "Aprovação Final";
            dto.StepOrder = 3;

            // Assert
            Assert.Equal("Aprovação Final", dto.StepName);
            Assert.Equal(3, dto.StepOrder);
        }
    }
}

using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class ProcessExecutionDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new ProcessExecutionDto();

            // Assert
            Assert.Equal(0, dto.ExecutionId);
            Assert.Equal(0, dto.ProcessId);
            Assert.Equal(0, dto.StepId);
            Assert.Null(dto.UserId);
            Assert.Equal("Pendente", dto.Status);
            Assert.Null(dto.StartedAt);
            Assert.Null(dto.CompletedAt);
            Assert.Null(dto.Remarks);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var startDate = new DateTime(2025, 11, 8, 10, 0, 0);
            var endDate = new DateTime(2025, 11, 8, 12, 0, 0);

            var dto = new ProcessExecutionDto
            {
                ExecutionId = 1,
                ProcessId = 2,
                StepId = 3,
                UserId = 99,
                Status = "Concluído",
                StartedAt = startDate,
                CompletedAt = endDate,
                Remarks = "Processo finalizado com sucesso"
            };

            // Act & Assert
            Assert.Equal(1, dto.ExecutionId);
            Assert.Equal(2, dto.ProcessId);
            Assert.Equal(3, dto.StepId);
            Assert.Equal(99, dto.UserId);
            Assert.Equal("Concluído", dto.Status);
            Assert.Equal(startDate, dto.StartedAt);
            Assert.Equal(endDate, dto.CompletedAt);
            Assert.Equal("Processo finalizado com sucesso", dto.Remarks);
        }

        [Fact]
        public void Status_ShouldDefaultToPendente_WhenNotExplicitlySet()
        {
            // Arrange
            var dto = new ProcessExecutionDto();

            // Assert
            Assert.Equal("Pendente", dto.Status);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new ProcessExecutionDto
            {
                UserId = null,
                StartedAt = null,
                CompletedAt = null,
                Remarks = null
            };

            // Assert
            Assert.Null(dto.UserId);
            Assert.Null(dto.StartedAt);
            Assert.Null(dto.CompletedAt);
            Assert.Null(dto.Remarks);
        }

        [Fact]
        public void Status_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new ProcessExecutionDto { Status = "Pendente" };

            // Act
            dto.Status = "Em Andamento";

            // Assert
            Assert.Equal("Em Andamento", dto.Status);
        }
    }
}

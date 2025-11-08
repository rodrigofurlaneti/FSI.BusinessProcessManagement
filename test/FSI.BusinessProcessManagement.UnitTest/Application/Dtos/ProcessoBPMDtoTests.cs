using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class ProcessoBPMDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new ProcessoBPMDto();

            // Assert
            Assert.Equal(0, dto.ProcessId);
            Assert.Null(dto.DepartmentId);
            Assert.Null(dto.ProcessName);
            Assert.Null(dto.Description);
            Assert.Null(dto.CreatedBy);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new ProcessoBPMDto
            {
                ProcessId = 100,
                DepartmentId = 20,
                ProcessName = "Aprovação de Contratos",
                Description = "Fluxo responsável por aprovar contratos de fornecedores",
                CreatedBy = 5
            };

            // Act & Assert
            Assert.Equal(100, dto.ProcessId);
            Assert.Equal(20, dto.DepartmentId);
            Assert.Equal("Aprovação de Contratos", dto.ProcessName);
            Assert.Equal("Fluxo responsável por aprovar contratos de fornecedores", dto.Description);
            Assert.Equal(5, dto.CreatedBy);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new ProcessoBPMDto
            {
                DepartmentId = null,
                ProcessName = null,
                Description = null,
                CreatedBy = null
            };

            // Assert
            Assert.Null(dto.DepartmentId);
            Assert.Null(dto.ProcessName);
            Assert.Null(dto.Description);
            Assert.Null(dto.CreatedBy);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new ProcessoBPMDto
            {
                ProcessName = "Antigo Processo",
                Description = "Descrição antiga"
            };

            // Act
            dto.ProcessName = "Novo Processo";
            dto.Description = "Descrição atualizada";

            // Assert
            Assert.Equal("Novo Processo", dto.ProcessName);
            Assert.Equal("Descrição atualizada", dto.Description);
        }
    }
}

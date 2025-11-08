using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class ProcessStepMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_All_Fields()
        {
            // Arrange
            var dto = new ProcessStepDto
            {
                ProcessId = 100,
                StepName = "Validação de Documentos",
                StepOrder = 2,
                AssignedRoleId = 50
            };

            // Act
            var entity = ProcessStepMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal(100, entity.ProcessId);
            Assert.Equal("Validação de Documentos", entity.StepName);
            Assert.Equal(2, entity.StepOrder);
            Assert.Equal(50, entity.AssignedRoleId);
        }

        [Fact]
        public void CopyToExisting_Should_Update_All_Mutable_Fields()
        {
            // Arrange
            var entity = new ProcessStep(processId: 10, stepName: "Antigo", stepOrder: 1, assignedRoleId: 5);
            var dto = new ProcessStepDto
            {
                StepName = "Novo Nome",
                StepOrder = 3,
                AssignedRoleId = 99
            };

            // Act
            ProcessStepMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal("Novo Nome", entity.StepName);
            Assert.Equal(3, entity.StepOrder);
            Assert.Equal(99, entity.AssignedRoleId);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new ProcessStep(processId: 20, stepName: "Aprovação", stepOrder: 4, assignedRoleId: 7);
            var expectedId = entity.Id;

            // Act
            var dto = ProcessStepMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.StepId);
            Assert.Equal(20, dto.ProcessId);
            Assert.Equal("Aprovação", dto.StepName);
            Assert.Equal(4, dto.StepOrder);
            Assert.Equal(7, dto.AssignedRoleId);
        }
    }
}
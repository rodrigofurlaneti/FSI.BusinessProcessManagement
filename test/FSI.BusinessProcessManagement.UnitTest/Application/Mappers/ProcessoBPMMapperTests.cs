using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class ProcessoBPMMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_All_Fields_And_Keep_Name()
        {
            // Arrange
            var dto = new ProcessoBPMDto
            {
                ProcessName = "Aprovação de Contratos",
                DepartmentId = 20,
                Description = "Fluxo de aprovação",
                CreatedBy = 5
            };

            // Act
            var entity = ProcessoBPMMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("Aprovação de Contratos", entity.Name);
            Assert.Equal(20, entity.DepartmentId);
            Assert.Equal("Fluxo de aprovação", entity.Description);
            Assert.Equal(5, entity.CreatedBy);
        }

        [Fact]
        public void CopyToExisting_Should_Update_Description_And_Department_Only()
        {
            var entity = new DomainProcess("Nome Original", 10, "Desc antiga", 7);

            var dto = new ProcessoBPMDto
            {
                ProcessName = "Novo Nome (ignorado pelo mapper)",
                DepartmentId = 50,
                Description = "Desc nova",
                CreatedBy = 999 
            };

            ProcessoBPMMapper.CopyToExisting(entity, dto);

            Assert.Equal("Nome Original", entity.Name); 
            Assert.Equal(50, entity.DepartmentId);
            Assert.Equal("Desc nova", entity.Description);
            Assert.Equal(7, entity.CreatedBy);
        }

        [Fact]
        public void CopyToExisting_Should_Accept_Nulls_For_Description_And_Department()
        {
            // Arrange
            var entity = new DomainProcess("Proc", 100, "Com descrição", 3);

            var dto = new ProcessoBPMDto
            {
                DepartmentId = null,
                Description = null
            };

            // Act
            ProcessoBPMMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Null(entity.DepartmentId);
            Assert.Null(entity.Description);
            Assert.Equal("Proc", entity.Name);
            Assert.Equal(3, entity.CreatedBy);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new DomainProcess("Financeiro", 77, "Fluxo de orçamento", 4);
            var expectedId = entity.Id;

            // Act
            var dto = ProcessoBPMMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.ProcessId);
            Assert.Equal(77, dto.DepartmentId);
            Assert.Equal("Financeiro", dto.ProcessName);
            Assert.Equal("Fluxo de orçamento", dto.Description);
            Assert.Equal(4, dto.CreatedBy);
        }
    }
}
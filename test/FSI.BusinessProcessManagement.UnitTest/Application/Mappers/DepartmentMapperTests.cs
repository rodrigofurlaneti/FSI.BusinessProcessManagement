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
    public class DepartmentMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_Name_And_Description()
        {
            var dto = new DepartmentDto
            {
                DepartmentName = "Tecnologia",
                Description = "Infra e sistemas"
            };

            var entity = DepartmentMapper.ToNewEntity(dto);

            Assert.NotNull(entity);
            Assert.Equal("Tecnologia", entity.Name);
            Assert.Equal("Infra e sistemas", entity.Description);
        }
        
        [Fact]
        public void CopyToExisting_Should_Update_All_Mutable_Fields()
        {
            // Arrange
            var entity = new Department("Antigo", "Desc antiga");
            var dto = new DepartmentDto
            {
                DepartmentName = "Novo Nome",
                Description = "Nova descrição"
            };

            // Act
            DepartmentMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal("Novo Nome", entity.Name);
            Assert.Equal("Nova descrição", entity.Description);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new Department("Financeiro", "Orçamento e custos");
            var expectedId = entity.Id;

            // Act
            var dto = DepartmentMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.DepartmentId);
            Assert.Equal("Financeiro", dto.DepartmentName);
            Assert.Equal("Orçamento e custos", dto.Description);
        }
    }
}
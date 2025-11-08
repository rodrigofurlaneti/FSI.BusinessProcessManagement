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
    public class RoleMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_Name_And_Description()
        {
            // Arrange
            var dto = new RoleDto
            {
                RoleName = "Administrador",
                Description = "Acesso total"
            };

            // Act
            var entity = RoleMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("Administrador", entity.Name);
            Assert.Equal("Acesso total", entity.Description);
        }

        [Fact]
        public void CopyToExisting_Should_Update_All_Mutable_Fields()
        {
            // Arrange
            var entity = new Role("Antigo", "Desc antiga");
            var dto = new RoleDto
            {
                RoleName = "Novo Nome",
                Description = "Nova descrição"
            };

            // Act
            RoleMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal("Novo Nome", entity.Name);
            Assert.Equal("Nova descrição", entity.Description);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new Role("Gerente", "Acesso a relatórios");
            var expectedId = entity.Id;

            // Act
            var dto = RoleMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.RoleId);
            Assert.Equal("Gerente", dto.RoleName);
            Assert.Equal("Acesso a relatórios", dto.Description);
        }
    }
}
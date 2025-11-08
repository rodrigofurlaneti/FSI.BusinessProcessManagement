using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class ScreenMapperTests
    {
        [Fact]
        public void ToNewEntity_Should_Map_Name_And_Description()
        {
            // Arrange
            var dto = new ScreenDto
            {
                ScreenName = "Dashboard",
                Description = "Tela inicial com KPIs"
            };

            // Act
            var entity = ScreenMapper.ToNewEntity(dto);

            // Assert
            Assert.NotNull(entity);
            Assert.Equal("Dashboard", entity.Name);
            Assert.Equal("Tela inicial com KPIs", entity.Description);
        }

        [Fact]
        public void CopyToExisting_Should_Update_All_Mutable_Fields()
        {
            // Arrange
            var entity = new Screen("Antiga", "Desc antiga");
            var dto = new ScreenDto
            {
                ScreenName = "Relatórios",
                Description = "Tela de relatórios"
            };

            // Act
            ScreenMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal("Relatórios", entity.Name);
            Assert.Equal("Tela de relatórios", entity.Description);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields_From_Entity()
        {
            // Arrange
            var entity = new Screen("Configurações", "Tela de configuração");
            var expectedId = entity.Id;

            // Act
            var dto = ScreenMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.ScreenId);
            Assert.Equal("Configurações", dto.ScreenName);
            Assert.Equal("Tela de configuração", dto.Description);
        }
    }
}

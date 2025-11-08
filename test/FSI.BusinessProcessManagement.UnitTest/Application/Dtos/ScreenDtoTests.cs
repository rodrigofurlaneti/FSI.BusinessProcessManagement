using FSI.BusinessProcessManagement.Application.Dtos;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Dtos
{
    public class ScreenDtoTests
    {
        [Fact]
        public void Constructor_ShouldInitializeProperties_WithDefaultValues()
        {
            // Arrange & Act
            var dto = new ScreenDto();

            // Assert
            Assert.Equal(0, dto.ScreenId);
            Assert.Null(dto.ScreenName);
            Assert.Null(dto.Description);
        }

        [Fact]
        public void Properties_ShouldHoldAssignedValues()
        {
            // Arrange
            var dto = new ScreenDto
            {
                ScreenId = 10,
                ScreenName = "Dashboard",
                Description = "Tela principal com resumo de informações"
            };

            // Act & Assert
            Assert.Equal(10, dto.ScreenId);
            Assert.Equal("Dashboard", dto.ScreenName);
            Assert.Equal("Tela principal com resumo de informações", dto.Description);
        }

        [Fact]
        public void NullableProperties_ShouldAcceptNullValues()
        {
            // Arrange
            var dto = new ScreenDto
            {
                ScreenId = 5,
                ScreenName = null,
                Description = null
            };

            // Assert
            Assert.Equal(5, dto.ScreenId);
            Assert.Null(dto.ScreenName);
            Assert.Null(dto.Description);
        }

        [Fact]
        public void Properties_CanBeChanged_AfterInitialization()
        {
            // Arrange
            var dto = new ScreenDto
            {
                ScreenName = "Configurações",
                Description = "Tela de configuração inicial"
            };

            // Act
            dto.ScreenName = "Relatórios";
            dto.Description = "Tela de relatórios e análises";

            // Assert
            Assert.Equal("Relatórios", dto.ScreenName);
            Assert.Equal("Tela de relatórios e análises", dto.Description);
        }
    }
}

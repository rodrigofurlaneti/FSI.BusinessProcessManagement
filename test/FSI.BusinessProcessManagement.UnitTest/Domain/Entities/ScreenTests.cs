using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class ScreenTests
    {
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Screen_Class_MustBeSealed_And_Inherit_BaseEntity()
        {
            var t = typeof(Screen);

            Assert.True(t.IsSealed, "Screen deve continuar sendo sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void Screen_Properties_MustExist_WithExpectedTypes_And_PrivateSetters()
        {
            var t = typeof(Screen);

            // Name
            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nameProp);
            Assert.Equal(typeof(string), nameProp!.PropertyType);
            Assert.NotNull(nameProp.GetGetMethod());
            Assert.Null(nameProp.GetSetMethod());
            var nameSetter = nameProp.GetSetMethod(true);
            Assert.NotNull(nameSetter);
            Assert.True(nameSetter!.IsPrivate, "Name deve manter 'private set;'.");

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType);
            Assert.NotNull(descProp.GetGetMethod());
            Assert.Null(descProp.GetSetMethod());
            var descSetter = descProp.GetSetMethod(true);
            Assert.NotNull(descSetter);
            Assert.True(descSetter!.IsPrivate, "Description deve manter 'private set;'.");
        }

        [Fact]
        public void Screen_MustHave_PrivateParameterlessCtor_And_PublicMainCtor()
        {
            var t = typeof(Screen);

            // Construtor private sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "O construtor vazio deve continuar private (EF).");

            // Construtor público (string name, string? description)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 2
                        && p[0].ParameterType == typeof(string)
                        && p[1].ParameterType == typeof(string);
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void Screen_PublicMethods_MustExist_WithExpectedSignatures()
        {
            var t = typeof(Screen);

            var setName = t.GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setName);
            Assert.Single(setName!.GetParameters());
            Assert.Equal(typeof(string), setName.GetParameters()[0].ParameterType);

            var setDesc = t.GetMethod("SetDescription", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setDesc);
            Assert.Single(setDesc!.GetParameters());
            Assert.Equal(typeof(string), setDesc.GetParameters()[0].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR / INICIALIZAÇÃO
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_TrimmedName_AndDescription()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var screen = new Screen("  Dashboard Central  ", "  Tela principal do sistema  ");

            // Assert
            Assert.Equal("Dashboard Central", screen.Name);
            Assert.Equal("Tela principal do sistema", screen.Description);
            Assert.NotNull(screen.UpdatedAt);
            Assert.InRange(screen.UpdatedAt!.Value, before, DateTime.UtcNow);
        }

        [Fact]
        public void Constructor_WithValidName_AndNullDescription_ShouldInitialize_OnlyName()
        {
            var screen = new Screen("  Relatórios  ");
            Assert.Equal("Relatórios", screen.Name);
            Assert.Null(screen.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_WithInvalidName_ShouldThrowDomainException(string invalidName)
        {
            var ex = Assert.Throws<DomainException>(() => new Screen(invalidName));
            Assert.Equal("Screen name is required.", ex.Message);
        }

        [Fact]
        public void Constructor_WithNameLongerThan150_ShouldThrowDomainException()
        {
            var longName = new string('A', 151);
            var ex = Assert.Throws<DomainException>(() => new Screen(longName));
            Assert.Equal("Screen name too long (max 150).", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetName
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetName_WithValidValue_ShouldTrim_And_Update_UpdatedAt()
        {
            var screen = new Screen("Home");
            var before = screen.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            screen.SetName("   Configurações do Sistema   ");

            Assert.Equal("Configurações do Sistema", screen.Name);
            Assert.NotNull(screen.UpdatedAt);
            Assert.True(screen.UpdatedAt >= before, "SetName deve chamar Touch().");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("     ")]
        public void SetName_WithNullOrWhitespace_ShouldThrowDomainException(string invalidName)
        {
            var screen = new Screen("Inicial");
            var ex = Assert.Throws<DomainException>(() => screen.SetName(invalidName));
            Assert.Equal("Screen name is required.", ex.Message);
        }

        [Fact]
        public void SetName_WithNameLongerThan150_ShouldThrowDomainException()
        {
            var screen = new Screen("Cadastro");
            var longName = new string('B', 151);
            var ex = Assert.Throws<DomainException>(() => screen.SetName(longName));
            Assert.Equal("Screen name too long (max 150).", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetDescription
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetDescription_WithValidValue_ShouldTrim_And_Touch()
        {
            var screen = new Screen("Painel Principal");
            var before = screen.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            screen.SetDescription("   Tela de controle operacional   ");

            Assert.Equal("Tela de controle operacional", screen.Description);
            Assert.NotNull(screen.UpdatedAt);
            Assert.True(screen.UpdatedAt >= before);
        }

        [Fact]
        public void SetDescription_WithNull_ShouldSetNull_And_Touch()
        {
            var screen = new Screen("Painel", "Desc inicial");
            var before = screen.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            screen.SetDescription(null);

            Assert.Null(screen.Description);
            Assert.NotNull(screen.UpdatedAt);
            Assert.True(screen.UpdatedAt >= before);
        }

        [Fact]
        public void SetDescription_WithWhitespace_ShouldResultInEmptyString()
        {
            var screen = new Screen("Login");
            screen.SetDescription("     ");
            Assert.Equal(string.Empty, screen.Description);
        }

        // -----------------------------------------------------------------------------------------
        // 5. MUTATION STABILITY / TOUCH()
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldNotThrow_And_AlwaysUpdateUpdatedAt()
        {
            var screen = new Screen("Dashboard");

            var ex1 = Record.Exception(() => screen.SetName("Configurações"));
            var after1 = screen.UpdatedAt;

            var ex2 = Record.Exception(() => screen.SetDescription("Gerenciamento de módulos"));
            var after2 = screen.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.NotNull(after1);
            Assert.NotNull(after2);
            Assert.True(after2 >= after1, "UpdatedAt deve avançar a cada mutação.");
        }
    }
}

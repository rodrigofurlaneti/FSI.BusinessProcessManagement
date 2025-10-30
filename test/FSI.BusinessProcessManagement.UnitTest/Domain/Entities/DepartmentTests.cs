using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class DepartmentTests
    {
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Department_Class_Must_Exist_And_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(Department);

            Assert.NotNull(t);

            Assert.True(t.IsSealed,
                "Department deve continuar sendo sealed. Se mudar, atualize este teste.");

            Assert.True(t.BaseType == typeof(BaseEntity),
                "Department deve continuar herdando BaseEntity. Se mudar a herança, atualize este teste.");
        }

        [Fact]
        public void Department_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(Department);

            // Name
            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nameProp);
            Assert.Equal(typeof(string), nameProp!.PropertyType);
            Assert.NotNull(nameProp.GetGetMethod());       // getter público
            Assert.Null(nameProp.GetSetMethod());          // NÃO tem set público
            var nameSetMethod = nameProp.GetSetMethod(true);
            Assert.NotNull(nameSetMethod);                 // tem set não-público
            Assert.True(nameSetMethod!.IsPrivate,
                "Name deve continuar com 'private set;'.");

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType); // string? em runtime é System.String
            Assert.NotNull(descProp.GetGetMethod());
            Assert.Null(descProp.GetSetMethod());
            var descSetMethod = descProp.GetSetMethod(true);
            Assert.NotNull(descSetMethod);
            Assert.True(descSetMethod!.IsPrivate,
                "Description deve continuar com 'private set;'.");
        }

        [Fact]
        public void Department_Must_Have_Private_Parameterless_Constructor_And_Public_Constructor_WithNameAndOptionalDescription()
        {
            var t = typeof(Department);

            // ctor privado sem parâmetros
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private para o ORM. Se mudar, atualize o teste.");

            // ctor público esperado (string name, string? description = null)
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
        public void Department_Public_Methods_Must_Exist_And_Signatures_Must_Remain()
        {
            var t = typeof(Department);

            // SetName(string name)
            var setName = t.GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setName);
            var pSetName = setName!.GetParameters();
            Assert.Single(pSetName);
            Assert.Equal(typeof(string), pSetName[0].ParameterType);

            // SetDescription(string? description)
            var setDesc = t.GetMethod("SetDescription", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setDesc);
            var pSetDesc = setDesc!.GetParameters();
            Assert.Single(pSetDesc);
            Assert.Equal(typeof(string), pSetDesc[0].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR (REGRAS INICIAIS)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_Valid_Name_WithoutDescription_ShouldInitialize_Name_Trimmed_And_DescriptionNull()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var dep = new Department("  Financeiro  ");

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal("Financeiro", dep.Name);
            Assert.Null(dep.Description);

            // UpdatedAt deve ter sido definido por causa do SetName(name) dentro do construtor
            Assert.NotNull(dep.UpdatedAt);
            Assert.InRange(dep.UpdatedAt!.Value, before, after);
        }

        [Fact]
        public void Constructor_Valid_Name_And_Description_ShouldInitialize_Trimmed_And_CallTouch()
        {
            // Arrange
            var dep = new Department("  TI  ", "   suporte de infraestrutura   ");

            // Assert
            Assert.Equal("TI", dep.Name);
            Assert.Equal("suporte de infraestrutura", dep.Description);

            // Importante:
            // - SetName() chama Touch()
            // - Depois o ctor faz "Description = description?.Trim();" SEM chamar Touch() de novo.
            // Então, UpdatedAt PRECISA ter sido setado ao menos uma vez pelo SetName()
            Assert.NotNull(dep.UpdatedAt);

            // UpdatedAt deve ser bem próximo de agora em UTC
            var nowUtc = DateTime.UtcNow;
            Assert.InRange(dep.UpdatedAt!.Value,
                nowUtc.AddMinutes(-1),
                nowUtc.AddMinutes(1));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_Invalid_Name_ShouldThrowDomainException(string invalidName)
        {
            Assert.Throws<DomainException>(() =>
                new Department(invalidName)
            );
        }

        [Fact]
        public void Constructor_NameLongerThan150Chars_ShouldThrowDomainException()
        {
            var tooLong = new string('X', 151);

            Assert.Throws<DomainException>(() =>
                new Department(tooLong)
            );
        }

        [Fact]
        public void Constructor_ShouldTrimDescription_WhenProvided()
        {
            var dep = new Department("Operações", "  área responsável por pátio e pista   ");

            Assert.Equal("Operações", dep.Name);
            Assert.Equal("área responsável por pátio e pista", dep.Description);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetName (REGRAS DE DOMÍNIO)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetName_WithValidValue_ShouldTrim_And_UpdateUpdatedAt()
        {
            // Arrange
            var dep = new Department("Inicial");
            var before = dep.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            dep.SetName("   Novos Processos   ");

            // Assert
            Assert.Equal("Novos Processos", dep.Name);

            Assert.NotNull(dep.UpdatedAt);
            Assert.True(dep.UpdatedAt >= before,
                "SetName deve chamar Touch(), que atualiza UpdatedAt para um valor igual ou maior.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_WithNullOrWhitespace_ShouldThrowDomainException(string invalidName)
        {
            var dep = new Department("RH");

            var ex = Assert.Throws<DomainException>(() => dep.SetName(invalidName));
            Assert.Equal("Department name is required.", ex.Message);
        }

        [Fact]
        public void SetName_WithMoreThan150Chars_ShouldThrowDomainException()
        {
            var dep = new Department("Contabilidade");

            string longName = new string('A', 151);

            var ex = Assert.Throws<DomainException>(() => dep.SetName(longName));
            Assert.Equal("Department name cannot be longer than 150 characters.", ex.Message);
        }

        [Fact]
        public void SetName_ShouldCallTouch_WithoutThrowing()
        {
            var dep = new Department("RH");

            var ex = Record.Exception(() => dep.SetName("Fiscal"));
            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetDescription (REGRAS DE DOMÍNIO)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetDescription_WithValidValue_ShouldTrim_And_UpdateUpdatedAt()
        {
            // Arrange
            var dep = new Department("Manutenção");

            var before = dep.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            // Act
            dep.SetDescription("   Equipe de manutenção pesada  ");

            // Assert
            Assert.Equal("Equipe de manutenção pesada", dep.Description);

            Assert.NotNull(dep.UpdatedAt);
            Assert.True(dep.UpdatedAt >= before,
                "SetDescription deve chamar Touch(), atualizando UpdatedAt.");
        }

        [Fact]
        public void SetDescription_WithNull_ShouldSetNull_And_Touch()
        {
            // Arrange
            var dep = new Department("Operações", "Algo qualquer");

            var before = dep.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            // Act
            dep.SetDescription(null);

            // Assert
            Assert.Null(dep.Description);
            Assert.NotNull(dep.UpdatedAt);
            Assert.True(dep.UpdatedAt >= before,
                "Mesmo limpando description para null, Touch() deve atualizar UpdatedAt.");
        }

        [Fact]
        public void SetDescription_WithWhitespace_ShouldTrimToEmptyStringThenAssignEmpty()
        {
            // Atenção: sua implementação atual faz:
            // Description = description?.Trim();
            // Ou seja:
            // - Se description é null -> Description vira null
            // - Se description é "   " -> description?.Trim() == "" -> Description vira ""
            //
            // Então Description NÃO vai virar null nesse caso,
            // ela vira string.Empty. Esse teste trava esse comportamento.

            var dep = new Department("Planejamento");

            dep.SetDescription("      ");

            Assert.Equal(string.Empty, dep.Description);
        }

        [Fact]
        public void SetDescription_ShouldCallTouch_WithoutThrowing()
        {
            var dep = new Department("Jurídico");

            var ex = Record.Exception(() => dep.SetDescription("Atuação cível e regulatória"));
            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // 5. TOUCH / AUDITORIA IMPLÍCITA
        //    Esse teste garante que os métodos públicos mutáveis não explodem por causa de Touch().
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_Should_NotThrow_And_Should_UpdateUpdatedAt()
        {
            var dep = new Department("Suprimentos");

            var ex1 = Record.Exception(() => dep.SetName("Compras e Contratos"));
            var afterName = dep.UpdatedAt;

            var ex2 = Record.Exception(() => dep.SetDescription("Responsável por contratos estratégicos"));
            var afterDesc = dep.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);

            Assert.NotNull(afterName);
            Assert.NotNull(afterDesc);
            Assert.True(afterDesc >= afterName,
                "Chamadas posteriores devem manter UpdatedAt atualizada cronologicamente.");
        }
    }
}

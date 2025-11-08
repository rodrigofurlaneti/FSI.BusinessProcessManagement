using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class BaseEntityTests
    {
        // Classe auxiliar apenas para teste (não vai pra produção)
        private class TestEntity : BaseEntity
        {
            // Vamos expor um método público que chama o Touch() protegido
            public void PublicTouch()
            {
                Touch();
            }

            // Vamos também permitir setar o Id artificialmente para validar os sets protected
            public void ForceSetId(long id)
            {
                // como o setter é protected, conseguimos setar aqui dentro da classe derivada
                Id = id;
            }
        }

        // ------------------------------------------------------------------------------------------------
        // 1. CONTRATO ESTRUTURAL
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void BaseEntity_Class_Must_Exist_And_Be_Abstract()
        {
            var t = typeof(BaseEntity);

            Assert.NotNull(t);
            Assert.True(t.IsAbstract,
                "BaseEntity deve continuar sendo abstract. Se mudar isso, atualize o teste.");
        }

        [Fact]
        public void BaseEntity_Properties_Must_Exist_With_Expected_Types_And_ProtectedSetters()
        {
            var t = typeof(BaseEntity);

            // Id
            var idProp = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(idProp);
            Assert.Equal(typeof(long), idProp!.PropertyType);
            Assert.NotNull(idProp.GetGetMethod()); // get público
            // set deve ser protegido
            var idSetMethod = idProp.GetSetMethod(nonPublic: true);
            Assert.NotNull(idSetMethod);
            Assert.True(idSetMethod!.IsFamily,
                "Id deve continuar com set protegido (protected set;).");
            Assert.Null(idProp.GetSetMethod(false)); // não deve ter set público

            // CreatedAt
            var createdAtProp = t.GetProperty("CreatedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(createdAtProp);
            Assert.Equal(typeof(DateTime), createdAtProp!.PropertyType);
            Assert.NotNull(createdAtProp.GetGetMethod());
            var createdAtSetMethod = createdAtProp.GetSetMethod(nonPublic: true);
            Assert.NotNull(createdAtSetMethod);
            Assert.True(createdAtSetMethod!.IsFamily,
                "CreatedAt deve continuar com set protegido (protected set;).");
            Assert.Null(createdAtProp.GetSetMethod(false)); // sem set público

            // UpdatedAt
            var updatedAtProp = t.GetProperty("UpdatedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(updatedAtProp);
            Assert.Equal(typeof(DateTime?), updatedAtProp!.PropertyType);
            Assert.NotNull(updatedAtProp.GetGetMethod());
            var updatedAtSetMethod = updatedAtProp.GetSetMethod(nonPublic: true);
            Assert.NotNull(updatedAtSetMethod);
            Assert.True(updatedAtSetMethod!.IsFamily,
                "UpdatedAt deve continuar com set protegido (protected set;).");
            Assert.Null(updatedAtProp.GetSetMethod(false)); // sem set público
        }

        [Fact]
        public void BaseEntity_Must_Have_Protected_Parameterless_Constructor()
        {
            var t = typeof(BaseEntity);

            // Procura por um ctor não público (protected conta como non-public) sem parâmetros
            var ctor = t
                .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(ctor);
            Assert.True(ctor!.IsFamily, // IsFamily == protected
                "O construtor sem parâmetros de BaseEntity deve continuar protected. Se mudar, atualize o teste.");
        }

        [Fact]
        public void BaseEntity_Must_Have_Protected_Void_Touch_Method()
        {
            var t = typeof(BaseEntity);

            var touchMethod = t.GetMethod("Touch",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(touchMethod);
            Assert.Equal(typeof(void), touchMethod!.ReturnType);

            // Touch deve continuar protected, não private, não public
            Assert.True(touchMethod.IsFamily,
                "Touch() deve continuar protected. Se mudar visibilidade, atualize o teste.");

            // Touch não deve receber parâmetros
            Assert.Empty(touchMethod.GetParameters());
        }

        // ------------------------------------------------------------------------------------------------
        // 2. COMPORTAMENTO DO CONSTRUTOR
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_ShouldInitialize_CreatedAt_With_Now_And_Leave_UpdatedAt_Null()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var entity = new TestEntity();

            var after = DateTime.UtcNow;

            // Assert
            Assert.True(entity.CreatedAt >= before && entity.CreatedAt <= after,
                "CreatedAt deve ser inicializado no construtor com DateTime.Now.");

            Assert.Null(entity.UpdatedAt);

            // Id padrão em long deve ser 0 (valor default)
            Assert.Equal(0, entity.Id);
        }

        // ------------------------------------------------------------------------------------------------
        // 3. COMPORTAMENTO DO Touch()
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Touch_ShouldUpdate_UpdatedAt_With_UtcNow_And_NotChange_CreatedAt()
        {
            // Arrange
            var e = new TestEntity();

            var createdAtBefore = e.CreatedAt;
            var updatedAtBefore = e.UpdatedAt; // deve ser null no começo
            Assert.Null(updatedAtBefore);

            var beforeUtcNow = DateTime.UtcNow;

            // Act
            e.PublicTouch(); // chama Touch() protegido por um wrapper público

            var afterUtcNow = DateTime.UtcNow;

            // Assert
            // UpdatedAt deve ter sido preenchido
            Assert.NotNull(e.UpdatedAt);
            Assert.True(e.UpdatedAt >= beforeUtcNow && e.UpdatedAt <= afterUtcNow,
                "UpdatedAt deve receber DateTime.UtcNow dentro de Touch().");

            // CreatedAt NÃO pode ser alterado
            Assert.Equal(createdAtBefore, e.CreatedAt);
        }

        [Fact]
        public void Touch_CalledMultipleTimes_ShouldMoveUpdatedAtForwardOrEqual()
        {
            // Arrange
            var e = new TestEntity();

            e.PublicTouch();
            var first = e.UpdatedAt;
            Assert.NotNull(first);

            System.Threading.Thread.Sleep(5);

            // Act
            e.PublicTouch();
            var second = e.UpdatedAt;
            Assert.NotNull(second);

            // Assert
            // second deve ser >= first (normalmente >, mas >= cobre casos de resolução baixa)
            Assert.True(second >= first,
                "Cada chamada de Touch() deve atualizar UpdatedAt para um valor igual ou mais recente.");
        }

        // ------------------------------------------------------------------------------------------------
        // 4. INTEGRIDADE DE ACESSO A ID
        //    -> Garante que o Id não tem set público e pode ser alterado apenas via classe derivada.
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Id_ShouldHave_ProtectedSetter_AllowingDerivedClassToAssign()
        {
            // Arrange
            var e = new TestEntity();

            Assert.Equal(0, e.Id);

            // Act
            e.ForceSetId(12345);

            // Assert
            Assert.Equal(12345, e.Id);
        }

        [Fact]
        public void Id_ShouldNotHave_PublicSetter()
        {
            var t = typeof(BaseEntity);
            var idProp = t.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(idProp);

            // Se tiver set público, GetSetMethod(false) != null.
            Assert.Null(idProp!.GetSetMethod(false));
        }

        // ------------------------------------------------------------------------------------------------
        // 5. REGRESSÃO DE MUDANÇA ACIDENTAL NA ASSINATURA
        //    -> Se alguém mudar o tipo da propriedade, esse teste já falha.
        //    -> Esse teste valida os tipos explicitamente outra vez (redundância intencional).
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Property_Types_ShouldRemain_Long_DateTime_NullableDateTime()
        {
            var e = new TestEntity();

            Assert.IsType<long>(e.Id);
            Assert.IsType<DateTime>(e.CreatedAt);

            if (e.UpdatedAt is not null)
            {
                Assert.IsType<DateTime>(e.UpdatedAt);
            }
        }
    }
}

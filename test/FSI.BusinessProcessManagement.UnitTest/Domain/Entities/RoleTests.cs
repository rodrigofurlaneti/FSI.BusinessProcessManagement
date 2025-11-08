using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class RoleTests
    {
        private static DateTime MinWhenNull(DateTime? dt) => dt ?? DateTime.MinValue;

        // ------------------------------------------------------------
        // 1) CONTRATO / ESTRUTURA
        // ------------------------------------------------------------

        [Fact]
        public void Role_Class_Must_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(Role);

            Assert.NotNull(t);
            Assert.True(t.IsSealed, "Role deve continuar sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void Role_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(Role);

            // Name
            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nameProp);
            Assert.Equal(typeof(string), nameProp!.PropertyType);
            Assert.True(nameProp.GetGetMethod()!.IsPublic);
            Assert.Null(nameProp.GetSetMethod());
            Assert.True(nameProp.GetSetMethod(true)!.IsPrivate);

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType); // string? -> System.String em runtime
            Assert.True(descProp.GetGetMethod()!.IsPublic);
            Assert.Null(descProp.GetSetMethod());
            Assert.True(descProp.GetSetMethod(true)!.IsPrivate);

            // UserRoles (somente get)
            var urProp = t.GetProperty("UserRoles", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(urProp);
            Assert.True(typeof(ICollection<UserRole>).IsAssignableFrom(urProp!.PropertyType));
            Assert.True(urProp.GetGetMethod()!.IsPublic);
            Assert.Null(urProp.GetSetMethod(true)); // sem set
        }

        [Fact]
        public void Role_Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(Role);

            // Ctor privado sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "Ctor vazio deve permanecer private (EF).");

            // Ctor público esperado: (string name, string? description = null)
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
        public void Role_Public_Methods_Signatures_Must_Remain()
        {
            var t = typeof(Role);

            var setName = t.GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setName);
            Assert.Equal(typeof(string), setName!.GetParameters()[0].ParameterType);

            var setDescription = t.GetMethod("SetDescription", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setDescription);
            Assert.Equal(typeof(string), setDescription!.GetParameters()[0].ParameterType);
        }

        // ------------------------------------------------------------
        // 2) CONSTRUTOR / ESTADO INICIAL
        // ------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArgs_ShouldInitialize_AndTrim_AndTouch()
        {
            var beforeUtc = DateTime.UtcNow;

            var role = new Role(
                name: "   Gestor   ",
                description: "  pode aprovar fluxos  "
            );

            var afterUtc = DateTime.UtcNow;

            Assert.Equal("Gestor", role.Name);
            Assert.Equal("pode aprovar fluxos", role.Description);

            Assert.NotNull(role.UserRoles);
            Assert.Empty(role.UserRoles);

            // SetName() chama Touch()
            Assert.NotNull(role.UpdatedAt);
            Assert.True(role.UpdatedAt!.Value >= beforeUtc && role.UpdatedAt!.Value <= afterUtc);
        }

        [Fact]
        public void Constructor_WithNullOrWhitespaceDescription_ShouldJustTrimToNull()
        {
            var r1 = new Role("Operador", null);
            Assert.Null(r1.Description);

            var r2 = new Role("Operador", "");
            Assert.Equal(string.Empty, r2.Description); // ""?.Trim() == ""

            var r3 = new Role("Operador", "   ");
            Assert.Equal(string.Empty, r3.Description); // "   "?.Trim() == ""
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidName_ShouldThrowDomainException(string invalid)
        {
            var ex = Assert.Throws<DomainException>(() => new Role(invalid));
            Assert.Equal("Role name is required.", ex.Message);
        }

        [Fact]
        public void Constructor_NameTooLong_ShouldThrowDomainException()
        {
            var tooLong = new string('X', 101);
            var ex = Assert.Throws<DomainException>(() => new Role(tooLong));
            Assert.Equal("Role name cannot be longer than 100 characters.", ex.Message);
        }

        // ------------------------------------------------------------
        // 3) SetName
        // ------------------------------------------------------------

        [Fact]
        public void SetName_WithValidName_ShouldTrim_And_Touch()
        {
            var role = new Role("Inicial");
            var before = MinWhenNull(role.UpdatedAt);

            role.SetName("   Novo Nome   ");
            var after = MinWhenNull(role.UpdatedAt);

            Assert.Equal("Novo Nome", role.Name);
            Assert.True(after > before, "SetName deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_WithInvalidName_ShouldThrowDomainException(string invalid)
        {
            var role = new Role("Valido");
            var ex = Assert.Throws<DomainException>(() => role.SetName(invalid));
            Assert.Equal("Role name is required.", ex.Message);
        }

        [Fact]
        public void SetName_WithTooLongName_ShouldThrowDomainException()
        {
            var role = new Role("Valido");
            var tooLong = new string('A', 101);

            var ex = Assert.Throws<DomainException>(() => role.SetName(tooLong));
            Assert.Equal("Role name cannot be longer than 100 characters.", ex.Message);
        }

        // ------------------------------------------------------------
        // 4) SetDescription
        // ------------------------------------------------------------

        [Fact]
        public void SetDescription_WithValue_ShouldTrim_And_Touch()
        {
            var role = new Role("Operador");
            var before = MinWhenNull(role.UpdatedAt);

            role.SetDescription("   algo   ");
            var after = MinWhenNull(role.UpdatedAt);

            Assert.Equal("algo", role.Description);
            Assert.True(after > before, "SetDescription deve chamar Touch().");
        }

        [Fact]
        public void SetDescription_WithNull_ShouldSetNull_And_Touch()
        {
            var role = new Role("Operador", "preenchido");
            var before = MinWhenNull(role.UpdatedAt);

            role.SetDescription(null);
            var after = MinWhenNull(role.UpdatedAt);

            Assert.Null(role.Description);
            Assert.True(after > before);
        }

        [Fact]
        public void SetDescription_WithWhitespace_ShouldBecomeEmptyString_And_Touch()
        {
            var role = new Role("Operador");
            var before = MinWhenNull(role.UpdatedAt);

            role.SetDescription("   "); // "   "?.Trim() => ""
            var after = MinWhenNull(role.UpdatedAt);

            Assert.Equal(string.Empty, role.Description);
            Assert.True(after > before);
        }

        // ------------------------------------------------------------
        // 5) Mutation stability (ordem de toques)
        // ------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldAlwaysAdvance_UpdatedAt_InOrder()
        {
            var role = new Role("A");

            var t0 = MinWhenNull(role.UpdatedAt);

            role.SetName("B");
            var t1 = MinWhenNull(role.UpdatedAt);
            Assert.True(t1 > t0);

            role.SetDescription("desc");
            var t2 = MinWhenNull(role.UpdatedAt);
            Assert.True(t2 > t1);

            role.SetDescription(null);
            var t3 = MinWhenNull(role.UpdatedAt);
            Assert.True(t3 > t2);
        }
    }
}

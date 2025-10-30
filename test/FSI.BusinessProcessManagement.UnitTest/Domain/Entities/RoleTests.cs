using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class RoleTests
    {
        [Fact]
        public void Role_Class_MustBeSealed_AndInherit_BaseEntity()
        {
            var t = typeof(Role);

            Assert.True(t.IsSealed, "Role deve continuar sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void Role_MustHaveExpectedProperties_WithPrivateSetters()
        {
            var t = typeof(Role);

            // Name
            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nameProp);
            Assert.Equal(typeof(string), nameProp!.PropertyType);

            var getName = nameProp.GetGetMethod();
            Assert.NotNull(getName);
            Assert.True(getName!.IsPublic);

            Assert.Null(nameProp.GetSetMethod());
            var setNamePrivate = nameProp.GetSetMethod(true);
            Assert.NotNull(setNamePrivate);
            Assert.True(setNamePrivate!.IsPrivate);

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType); // string? em C# vira string em runtime

            var getDesc = descProp.GetGetMethod();
            Assert.NotNull(getDesc);
            Assert.True(getDesc!.IsPublic);

            Assert.Null(descProp.GetSetMethod());
            var setDescPrivate = descProp.GetSetMethod(true);
            Assert.NotNull(setDescPrivate);
            Assert.True(setDescPrivate!.IsPrivate);

            // UserRoles
            var urProp = t.GetProperty("UserRoles", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(urProp);
            Assert.True(typeof(System.Collections.Generic.ICollection<UserRole>).IsAssignableFrom(urProp!.PropertyType));

            var getUr = urProp.GetGetMethod();
            Assert.NotNull(getUr);
            Assert.True(getUr!.IsPublic);

            // não deve ter setter público nem privado (só get)
            Assert.Null(urProp.GetSetMethod());
            Assert.Null(urProp.GetSetMethod(true));
        }

        [Fact]
        public void Role_MustHave_PrivateParameterlessCtor_And_PublicCtor_Name_Description()
        {
            var t = typeof(Role);

            // ctor private sem parâmetros
            var privateCtor = t.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate);

            // ctor público esperado: Role(string name, string? description = null)
            var publicCtor = t.GetConstructors(BindingFlags.Public | BindingFlags.Instance)
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
        public void Constructor_ValidName_ShouldInitializeFields_TrimName_TrimDescription_UpdateAudit()
        {
            // arrange
            var before = DateTime.UtcNow;

            // act
            var role = new Role("  Admin Master  ", "  Pode tudo  ");

            var after = DateTime.UtcNow;

            // assert
            Assert.Equal("Admin Master", role.Name);
            Assert.Equal("Pode tudo", role.Description);

            // CreatedAt vem de BaseEntity().CreatedAt = DateTime.Now no ctor base
            Assert.True(role.CreatedAt > DateTime.Now.AddMinutes(-1));
            Assert.True(role.CreatedAt <= DateTime.Now);

            // SetName() chama Touch() => UpdatedAt deve existir e ser recente
            Assert.NotNull(role.UpdatedAt);
            Assert.InRange(role.UpdatedAt!.Value, before, after);
        }

        [Fact]
        public void Constructor_ValidName_WithoutDescription_ShouldKeepDescriptionNull()
        {
            var role = new Role("  Supervisor  ", null);

            Assert.Equal("Supervisor", role.Name);

            // <- ESTA LINHA ERA O TEU ERRO: NÃO PODE USAR Assert.Null(x, "msg")
            Assert.Null(role.Description);

            // UpdatedAt já deve ter sido setado por Touch() dentro de SetName()
            Assert.NotNull(role.UpdatedAt);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidName_ShouldThrowDomainException(string badName)
        {
            var ex = Assert.Throws<DomainException>(() => new Role(badName, "desc"));
            Assert.Equal("Role name is required.", ex.Message);
        }

        [Fact]
        public void SetName_ShouldTrim_AndUpdate_UpdatedAt()
        {
            var role = new Role("Original", "abc");
            var before = role.UpdatedAt;

            Thread.Sleep(5);
            role.SetName("   Novo Nome   ");

            Assert.Equal("Novo Nome", role.Name);
            Assert.NotNull(role.UpdatedAt);
            Assert.True(role.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_Invalid_ShouldThrowDomainException(string badName)
        {
            var role = new Role("ok", "x");

            var ex = Assert.Throws<DomainException>(() => role.SetName(badName));
            Assert.Equal("Role name is required.", ex.Message);
        }

        [Fact]
        public void SetDescription_ShouldTrim_UpdateDescription_AndTouch()
        {
            var role = new Role("Cargo", null);
            var before = role.UpdatedAt;

            Thread.Sleep(5);
            role.SetDescription("  Gerencia processos críticos  ");

            Assert.Equal("Gerencia processos críticos", role.Description);
            Assert.NotNull(role.UpdatedAt);
            Assert.True(role.UpdatedAt >= before);
        }

        [Fact]
        public void SetDescription_CanBeNull_AndShouldTouch()
        {
            var role = new Role("Cargo", "desc");
            var before = role.UpdatedAt;

            Thread.Sleep(5);
            role.SetDescription(null);

            // <- cuidado aqui: depois de SetDescription(null) a propriedade vira null
            Assert.Null(role.Description);

            Assert.NotNull(role.UpdatedAt);
            Assert.True(role.UpdatedAt >= before);
        }
    }
}

using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class UserRoleTests
    {
        private static DateTime MinWhenNull(DateTime? dt) => dt ?? DateTime.MinValue;

        // ------------------------------------------------------------
        // 1) CONTRATO / ESTRUTURA
        // ------------------------------------------------------------

        [Fact]
        public void UserRole_Class_Must_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(UserRole);

            Assert.NotNull(t);
            Assert.True(t.IsSealed, "UserRole deve continuar sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void UserRole_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(UserRole);

            // UserId
            var userId = t.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userId);
            Assert.Equal(typeof(long), userId!.PropertyType);
            Assert.True(userId.GetGetMethod()!.IsPublic);
            Assert.Null(userId.GetSetMethod());
            Assert.True(userId.GetSetMethod(true)!.IsPrivate);

            // RoleId
            var roleId = t.GetProperty("RoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleId);
            Assert.Equal(typeof(long), roleId!.PropertyType);
            Assert.True(roleId.GetGetMethod()!.IsPublic);
            Assert.Null(roleId.GetSetMethod());
            Assert.True(roleId.GetSetMethod(true)!.IsPrivate);

            // AssignedAt
            var assignedAt = t.GetProperty("AssignedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(assignedAt);
            Assert.Equal(typeof(DateTime), assignedAt!.PropertyType);
            Assert.True(assignedAt.GetGetMethod()!.IsPublic);
            Assert.Null(assignedAt.GetSetMethod());
            Assert.True(assignedAt.GetSetMethod(true)!.IsPrivate);

            // User (navegação)
            var userProp = t.GetProperty("User", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userProp);
            Assert.Equal(typeof(User), userProp!.PropertyType);
            Assert.True(userProp.GetGetMethod()!.IsPublic);
            Assert.Null(userProp.GetSetMethod());
            Assert.True(userProp.GetSetMethod(true)!.IsPrivate);

            // Role (navegação)
            var roleProp = t.GetProperty("Role", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleProp);
            Assert.Equal(typeof(Role), roleProp!.PropertyType);
            Assert.True(roleProp.GetGetMethod()!.IsPublic);
            Assert.Null(roleProp.GetSetMethod());
            Assert.True(roleProp.GetSetMethod(true)!.IsPrivate);
        }

        [Fact]
        public void Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(UserRole);

            // Ctor privado sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "Ctor vazio deve permanecer private (EF).");

            // Ctor público esperado: (long userId, long roleId)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 2
                           && p[0].ParameterType == typeof(long)
                           && p[1].ParameterType == typeof(long);
                });
            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void Public_Methods_Signatures_Must_Remain()
        {
            var t = typeof(UserRole);

            var setUser = t.GetMethod("SetUser", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setUser);
            Assert.Equal(typeof(long), setUser!.GetParameters()[0].ParameterType);

            var setRole = t.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setRole);
            Assert.Equal(typeof(long), setRole!.GetParameters()[0].ParameterType);
        }

        // ------------------------------------------------------------
        // 2) CONSTRUTOR / ESTADO INICIAL
        // ------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArgs_ShouldInitialize_AllFields_AndTouch()
        {
            var beforeUtc = DateTime.UtcNow;

            var ur = new UserRole(userId: 10, roleId: 20);

            var afterUtc = DateTime.UtcNow;

            Assert.Equal(10, ur.UserId);
            Assert.Equal(20, ur.RoleId);

            // AssignedAt definido no construtor (UtcNow)
            Assert.InRange(ur.AssignedAt, beforeUtc, afterUtc);

            // Navegações não são populadas no construtor
            Assert.Null(ur.User);
            Assert.Null(ur.Role);

            // SetUser/SetRole chamam Touch()
            Assert.NotNull(ur.UpdatedAt);
            Assert.True(ur.UpdatedAt!.Value >= beforeUtc && ur.UpdatedAt!.Value <= afterUtc);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidUser_ShouldThrow(long invalidUserId)
        {
            var ex = Assert.Throws<DomainException>(() => new UserRole(invalidUserId, roleId: 1));
            Assert.Equal("Invalid UserId.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidRole_ShouldThrow(long invalidRoleId)
        {
            var ex = Assert.Throws<DomainException>(() => new UserRole(userId: 1, roleId: invalidRoleId));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // ------------------------------------------------------------
        // 3) SetUser
        // ------------------------------------------------------------

        [Fact]
        public void SetUser_WithValidValue_ShouldAssign_And_Touch()
        {
            var ur = new UserRole(1, 2);
            var before = MinWhenNull(ur.UpdatedAt);

            ur.SetUser(99);
            var after = MinWhenNull(ur.UpdatedAt);

            Assert.Equal(99, ur.UserId);
            Assert.True(after > before, "SetUser deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetUser_WithInvalidValue_ShouldThrow(long invalidUserId)
        {
            var ur = new UserRole(1, 2);
            var ex = Assert.Throws<DomainException>(() => ur.SetUser(invalidUserId));
            Assert.Equal("Invalid UserId.", ex.Message);
        }

        // ------------------------------------------------------------
        // 4) SetRole
        // ------------------------------------------------------------

        [Fact]
        public void SetRole_WithValidValue_ShouldAssign_And_Touch()
        {
            var ur = new UserRole(1, 2);
            var before = MinWhenNull(ur.UpdatedAt);

            ur.SetRole(77);
            var after = MinWhenNull(ur.UpdatedAt);

            Assert.Equal(77, ur.RoleId);
            Assert.True(after > before, "SetRole deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetRole_WithInvalidValue_ShouldThrow(long invalidRoleId)
        {
            var ur = new UserRole(1, 2);
            var ex = Assert.Throws<DomainException>(() => ur.SetRole(invalidRoleId));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // ------------------------------------------------------------
        // 5) Mutation stability (ordem de toques)
        // ------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldAlwaysAdvance_UpdatedAt_InOrder()
        {
            var ur = new UserRole(1, 2);

            var t0 = MinWhenNull(ur.UpdatedAt);

            ur.SetUser(10);
            var t1 = MinWhenNull(ur.UpdatedAt);
            Assert.True(t1 > t0);

            ur.SetRole(20);
            var t2 = MinWhenNull(ur.UpdatedAt);
            Assert.True(t2 > t1);
        }

        [Fact]
        public void Create_WithNavigations_ShouldBindUserAndRole_AndKeepFKsConsistent()
        {
            var user = (User)Activator.CreateInstance(typeof(User), nonPublic: true)!;
            var role = (Role)Activator.CreateInstance(typeof(Role), nonPublic: true)!;

            typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(user, 10L);
            typeof(BaseEntity).GetProperty("Id", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)!
                .SetValue(role, 20L);

            var ur = UserRole.Create(user, role);

            Assert.Equal(10, ur.UserId);
            Assert.Equal(20, ur.RoleId);
            Assert.Same(user, ur.User);
            Assert.Same(role, ur.Role);
        }
    }
}

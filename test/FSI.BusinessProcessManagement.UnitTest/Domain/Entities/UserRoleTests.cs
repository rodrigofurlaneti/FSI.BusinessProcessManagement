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
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void UserRole_Class_MustBeSealed_And_Inherit_BaseEntity()
        {
            var t = typeof(UserRole);

            Assert.True(t.IsSealed, "UserRole deve continuar sendo sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void UserRole_Properties_MustExist_WithExpectedTypes_And_PrivateSetters()
        {
            var t = typeof(UserRole);

            // UserId
            var userIdProp = t.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userIdProp);
            Assert.Equal(typeof(long), userIdProp!.PropertyType);
            Assert.NotNull(userIdProp.GetGetMethod());
            Assert.Null(userIdProp.GetSetMethod());
            var userIdSetter = userIdProp.GetSetMethod(true);
            Assert.NotNull(userIdSetter);
            Assert.True(userIdSetter!.IsPrivate, "UserId deve manter 'private set;'.");

            // RoleId
            var roleIdProp = t.GetProperty("RoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleIdProp);
            Assert.Equal(typeof(long), roleIdProp!.PropertyType);
            Assert.NotNull(roleIdProp.GetGetMethod());
            Assert.Null(roleIdProp.GetSetMethod());
            var roleIdSetter = roleIdProp.GetSetMethod(true);
            Assert.NotNull(roleIdSetter);
            Assert.True(roleIdSetter!.IsPrivate, "RoleId deve manter 'private set;'.");

            // AssignedAt
            var assignedAtProp = t.GetProperty("AssignedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(assignedAtProp);
            Assert.Equal(typeof(DateTime), assignedAtProp!.PropertyType);
            Assert.NotNull(assignedAtProp.GetGetMethod());
            Assert.Null(assignedAtProp.GetSetMethod());
            var assignedAtSetter = assignedAtProp.GetSetMethod(true);
            Assert.NotNull(assignedAtSetter);
            Assert.True(assignedAtSetter!.IsPrivate,
                "AssignedAt deve manter 'private set;'.");

            // User (nav prop)
            var userProp = t.GetProperty("User", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userProp);
            Assert.Equal(typeof(User), userProp!.PropertyType);
            Assert.NotNull(userProp.GetGetMethod());
            Assert.Null(userProp.GetSetMethod());
            var userSetter = userProp.GetSetMethod(true);
            Assert.NotNull(userSetter);
            Assert.True(userSetter!.IsPrivate,
                "User navigation deve manter 'private set;'.");

            // Role (nav prop)
            var roleProp = t.GetProperty("Role", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleProp);
            Assert.Equal(typeof(Role), roleProp!.PropertyType);
            Assert.NotNull(roleProp.GetGetMethod());
            Assert.Null(roleProp.GetSetMethod());
            var roleSetter = roleProp.GetSetMethod(true);
            Assert.NotNull(roleSetter);
            Assert.True(roleSetter!.IsPrivate,
                "Role navigation deve manter 'private set;'.");
        }

        [Fact]
        public void UserRole_MustHave_PrivateParameterlessCtor_And_PublicCtor_WithExpectedSignature()
        {
            var t = typeof(UserRole);

            // construtor privado sem parâmetros (para EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private para EF.");

            // construtor público esperado: UserRole(long userId, long roleId)
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
        public void UserRole_PublicMethods_MustExist_WithExpectedSignatures()
        {
            var t = typeof(UserRole);

            // SetUser(long userId)
            var setUser = t.GetMethod("SetUser", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setUser);
            var suParams = setUser!.GetParameters();
            Assert.Single(suParams);
            Assert.Equal(typeof(long), suParams[0].ParameterType);

            // SetRole(long roleId)
            var setRole = t.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setRole);
            var srParams = setRole!.GetParameters();
            Assert.Single(srParams);
            Assert.Equal(typeof(long), srParams[0].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR / INICIALIZAÇÃO
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidIds_ShouldInitializeFields_AndAssignAssignedAtUtcNow_AndTouch()
        {
            // Arrange
            var beforeUtc = DateTime.UtcNow;

            // Act
            var userRole = new UserRole(userId: 10, roleId: 20);

            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal(10, userRole.UserId);
            Assert.Equal(20, userRole.RoleId);

            // AssignedAt deve ser definido no construtor com UtcNow
            Assert.InRange(userRole.AssignedAt, beforeUtc, afterUtc);

            // Navegações começam em null! (não populadas automaticamente)
            // null! é tratado como "será setado pelo EF", então aqui pode ainda estar null em runtime puro.
            // Não vamos forçar um valor aqui, apenas garantir que as props existem.
            Assert.True(userRole.User == null || userRole.User is User);
            Assert.True(userRole.Role == null || userRole.Role is Role);

            // SetUser e SetRole chamam Touch() -> então UpdatedAt precisa ter sido preenchido
            Assert.NotNull(userRole.UpdatedAt);
            Assert.InRange(userRole.UpdatedAt!.Value, beforeUtc, afterUtc);

            // CreatedAt inicializado no BaseEntity
            Assert.True(userRole.CreatedAt <= DateTime.Now && userRole.CreatedAt > DateTime.Now.AddMinutes(-1));
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-999, 1)]
        public void Constructor_WithInvalidUserId_ShouldThrowDomainException(long invalidUserId, long validRoleId)
        {
            var ex = Assert.Throws<DomainException>(() => new UserRole(invalidUserId, validRoleId));
            Assert.Equal("Invalid UserId.", ex.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        [InlineData(1, -999)]
        public void Constructor_WithInvalidRoleId_ShouldThrowDomainException(long validUserId, long invalidRoleId)
        {
            var ex = Assert.Throws<DomainException>(() => new UserRole(validUserId, invalidRoleId));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetUser
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetUser_WithValidValue_ShouldUpdateUserId_AndTouch()
        {
            // Arrange
            var ur = new UserRole(10, 20);
            var beforeUpdatedAt = ur.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            ur.SetUser(777);

            // Assert
            Assert.Equal(777, ur.UserId);
            Assert.NotNull(ur.UpdatedAt);
            Assert.True(ur.UpdatedAt >= beforeUpdatedAt,
                "SetUser deve chamar Touch() e atualizar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void SetUser_WithInvalid_ShouldThrowDomainException(long invalidUserId)
        {
            var ur = new UserRole(10, 20);

            var ex = Assert.Throws<DomainException>(() => ur.SetUser(invalidUserId));
            Assert.Equal("Invalid UserId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetRole
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetRole_WithValidValue_ShouldUpdateRoleId_AndTouch()
        {
            // Arrange
            var ur = new UserRole(10, 20);
            var beforeUpdatedAt = ur.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            ur.SetRole(999);

            // Assert
            Assert.Equal(999, ur.RoleId);
            Assert.NotNull(ur.UpdatedAt);
            Assert.True(ur.UpdatedAt >= beforeUpdatedAt,
                "SetRole deve chamar Touch() e atualizar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void SetRole_WithInvalid_ShouldThrowDomainException(long invalidRoleId)
        {
            var ur = new UserRole(10, 20);

            var ex = Assert.Throws<DomainException>(() => ur.SetRole(invalidRoleId));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 5. MUTATION STABILITY / TOUCH()
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldNotThrow_And_ShouldAdvanceUpdatedAt()
        {
            var ur = new UserRole(10, 20);

            var ex1 = Record.Exception(() => ur.SetUser(111));
            var ts1 = ur.UpdatedAt;

            var ex2 = Record.Exception(() => ur.SetRole(222));
            var ts2 = ur.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);

            Assert.NotNull(ts1);
            Assert.NotNull(ts2);

            Assert.True(ts2 >= ts1,
                "UpdatedAt deve avançar após segunda mutação (SetRole depois de SetUser).");
        }
    }
}

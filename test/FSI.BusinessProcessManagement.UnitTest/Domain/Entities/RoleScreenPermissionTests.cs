using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class RoleScreenPermissionTests
    {
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void RoleScreenPermission_Class_MustBeSealed_And_Inherit_BaseEntity()
        {
            var t = typeof(RoleScreenPermission);

            Assert.NotNull(t);

            Assert.True(t.IsSealed,
                "RoleScreenPermission deve continuar sealed. Se mudar, atualize este teste.");

            Assert.True(t.BaseType == typeof(BaseEntity),
                "RoleScreenPermission deve continuar herdando BaseEntity. Se mudar, atualize este teste.");
        }

        [Fact]
        public void RoleScreenPermission_Properties_MustExist_WithExpectedTypes_And_PrivateSetters()
        {
            var t = typeof(RoleScreenPermission);

            // RoleId
            var roleIdProp = t.GetProperty("RoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleIdProp);
            Assert.Equal(typeof(long), roleIdProp!.PropertyType);
            Assert.NotNull(roleIdProp.GetGetMethod());  // getter público
            Assert.Null(roleIdProp.GetSetMethod());     // sem setter público
            var roleIdSet = roleIdProp.GetSetMethod(true);
            Assert.NotNull(roleIdSet);                  // tem setter não público
            Assert.True(roleIdSet!.IsPrivate, "RoleId deve manter 'private set;'.");

            // ScreenId
            var screenIdProp = t.GetProperty("ScreenId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(screenIdProp);
            Assert.Equal(typeof(long), screenIdProp!.PropertyType);
            Assert.NotNull(screenIdProp.GetGetMethod());
            Assert.Null(screenIdProp.GetSetMethod());
            var screenIdSet = screenIdProp.GetSetMethod(true);
            Assert.NotNull(screenIdSet);
            Assert.True(screenIdSet!.IsPrivate, "ScreenId deve manter 'private set;'.");

            // CanView
            var canViewProp = t.GetProperty("CanView", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canViewProp);
            Assert.Equal(typeof(bool), canViewProp!.PropertyType);
            Assert.NotNull(canViewProp.GetGetMethod());
            Assert.Null(canViewProp.GetSetMethod());
            var canViewSet = canViewProp.GetSetMethod(true);
            Assert.NotNull(canViewSet);
            Assert.True(canViewSet!.IsPrivate, "CanView deve manter 'private set;'.");

            // CanCreate
            var canCreateProp = t.GetProperty("CanCreate", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canCreateProp);
            Assert.Equal(typeof(bool), canCreateProp!.PropertyType);
            Assert.NotNull(canCreateProp.GetGetMethod());
            Assert.Null(canCreateProp.GetSetMethod());
            var canCreateSet = canCreateProp.GetSetMethod(true);
            Assert.NotNull(canCreateSet);
            Assert.True(canCreateSet!.IsPrivate, "CanCreate deve manter 'private set;'.");

            // CanEdit
            var canEditProp = t.GetProperty("CanEdit", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canEditProp);
            Assert.Equal(typeof(bool), canEditProp!.PropertyType);
            Assert.NotNull(canEditProp.GetGetMethod());
            Assert.Null(canEditProp.GetSetMethod());
            var canEditSet = canEditProp.GetSetMethod(true);
            Assert.NotNull(canEditSet);
            Assert.True(canEditSet!.IsPrivate, "CanEdit deve manter 'private set;'.");

            // CanDelete
            var canDeleteProp = t.GetProperty("CanDelete", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canDeleteProp);
            Assert.Equal(typeof(bool), canDeleteProp!.PropertyType);
            Assert.NotNull(canDeleteProp.GetGetMethod());
            Assert.Null(canDeleteProp.GetSetMethod());
            var canDeleteSet = canDeleteProp.GetSetMethod(true);
            Assert.NotNull(canDeleteSet);
            Assert.True(canDeleteSet!.IsPrivate, "CanDelete deve manter 'private set;'.");
        }

        [Fact]
        public void RoleScreenPermission_MustHave_PrivateParameterlessCtor_And_PublicCtor_WithExpectedSignature()
        {
            var t = typeof(RoleScreenPermission);

            // Construtor private sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private (requisito EF). Se mudar, atualize este teste.");

            // Construtor público esperado:
            // (long roleId, long screenId, bool canView, bool canCreate, bool canEdit, bool canDelete)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 6
                        && p[0].ParameterType == typeof(long)
                        && p[1].ParameterType == typeof(long)
                        && p[2].ParameterType == typeof(bool)
                        && p[3].ParameterType == typeof(bool)
                        && p[4].ParameterType == typeof(bool)
                        && p[5].ParameterType == typeof(bool);
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void RoleScreenPermission_PublicMethods_MustExist_WithExpectedSignatures()
        {
            var t = typeof(RoleScreenPermission);

            // SetRole(long roleId)
            var setRole = t.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setRole);
            var setRoleParams = setRole!.GetParameters();
            Assert.Single(setRoleParams);
            Assert.Equal(typeof(long), setRoleParams[0].ParameterType);

            // SetScreen(long screenId)
            var setScreen = t.GetMethod("SetScreen", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setScreen);
            var setScreenParams = setScreen!.GetParameters();
            Assert.Single(setScreenParams);
            Assert.Equal(typeof(long), setScreenParams[0].ParameterType);

            // SetPermissions(bool view, bool create, bool edit, bool delete)
            var setPermissions = t.GetMethod("SetPermissions", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setPermissions);
            var setPermParams = setPermissions!.GetParameters();
            Assert.Equal(4, setPermParams.Length);
            Assert.Equal(typeof(bool), setPermParams[0].ParameterType);
            Assert.Equal(typeof(bool), setPermParams[1].ParameterType);
            Assert.Equal(typeof(bool), setPermParams[2].ParameterType);
            Assert.Equal(typeof(bool), setPermParams[3].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR / INICIALIZAÇÃO
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_Properties_AndCallTouch()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var perm = new RoleScreenPermission(
                roleId: 10,
                screenId: 20,
                canView: true,
                canCreate: false,
                canEdit: true,
                canDelete: false
            );

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(10, perm.RoleId);
            Assert.Equal(20, perm.ScreenId);
            Assert.True(perm.CanView);
            Assert.False(perm.CanCreate);
            Assert.True(perm.CanEdit);
            Assert.False(perm.CanDelete);

            // Como o construtor chama SetRole, SetScreen e SetPermissions,
            // e todos chamam Touch(), UpdatedAt já deve ter sido preenchido.
            Assert.NotNull(perm.UpdatedAt);
            Assert.InRange(perm.UpdatedAt!.Value, before, after);

            // CreatedAt é inicializado em BaseEntity
            Assert.True(perm.CreatedAt <= DateTime.Now && perm.CreatedAt > DateTime.Now.AddMinutes(-1));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void Constructor_InvalidRoleId_ShouldThrowDomainException(long invalidRole)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new RoleScreenPermission(
                    roleId: invalidRole,
                    screenId: 2,
                    canView: true,
                    canCreate: false,
                    canEdit: false,
                    canDelete: false
                )
            );

            Assert.Equal("Invalid role.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void Constructor_InvalidScreenId_ShouldThrowDomainException(long invalidScreen)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new RoleScreenPermission(
                    roleId: 1,
                    screenId: invalidScreen,
                    canView: true,
                    canCreate: true,
                    canEdit: false,
                    canDelete: false
                )
            );

            Assert.Equal("Invalid screen.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetRole
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetRole_WithValidValue_ShouldUpdateRoleId_AndTouch()
        {
            // Arrange
            var perm = new RoleScreenPermission(1, 2, true, true, false, false);
            var beforeUpdatedAt = perm.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            perm.SetRole(777);

            // Assert
            Assert.Equal(777, perm.RoleId);
            Assert.NotNull(perm.UpdatedAt);
            Assert.True(perm.UpdatedAt >= beforeUpdatedAt,
                "SetRole deve chamar Touch() e atualizar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-42)]
        public void SetRole_WithInvalidValue_ShouldThrowDomainException(long invalidRole)
        {
            var perm = new RoleScreenPermission(1, 2, true, true, false, false);

            var ex = Assert.Throws<DomainException>(() => perm.SetRole(invalidRole));
            Assert.Equal("Invalid role.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetScreen
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetScreen_WithValidValue_ShouldUpdateScreenId_AndTouch()
        {
            // Arrange
            var perm = new RoleScreenPermission(1, 2, true, false, false, false);
            var beforeUpdatedAt = perm.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            perm.SetScreen(999);

            // Assert
            Assert.Equal(999, perm.ScreenId);
            Assert.NotNull(perm.UpdatedAt);
            Assert.True(perm.UpdatedAt >= beforeUpdatedAt,
                "SetScreen deve chamar Touch() e atualizar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void SetScreen_WithInvalidValue_ShouldThrowDomainException(long invalidScreen)
        {
            var perm = new RoleScreenPermission(1, 2, true, false, false, false);

            var ex = Assert.Throws<DomainException>(() => perm.SetScreen(invalidScreen));
            Assert.Equal("Invalid screen.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 5. SetPermissions
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetPermissions_ShouldUpdateAllFlags_AndTouch()
        {
            // Arrange
            var perm = new RoleScreenPermission(
                roleId: 10,
                screenId: 20,
                canView: false,
                canCreate: false,
                canEdit: false,
                canDelete: false
            );

            var beforeUpdatedAt = perm.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            perm.SetPermissions(
                view: true,
                create: true,
                edit: false,
                delete: true
            );

            // Assert
            Assert.True(perm.CanView);
            Assert.True(perm.CanCreate);
            Assert.False(perm.CanEdit);
            Assert.True(perm.CanDelete);

            Assert.NotNull(perm.UpdatedAt);
            Assert.True(perm.UpdatedAt >= beforeUpdatedAt,
                "SetPermissions deve chamar Touch() e atualizar UpdatedAt.");
        }

        // -----------------------------------------------------------------------------------------
        // 6. MUTATION STABILITY / TOUCH()
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldNotThrow_And_ShouldMonotonicallyAdvanceUpdatedAt()
        {
            // Arrange
            var perm = new RoleScreenPermission(
                roleId: 10,
                screenId: 20,
                canView: false,
                canCreate: false,
                canEdit: false,
                canDelete: false
            );

            // Act
            var ex1 = Record.Exception(() => perm.SetRole(111));
            var after1 = perm.UpdatedAt;

            var ex2 = Record.Exception(() => perm.SetScreen(222));
            var after2 = perm.UpdatedAt;

            var ex3 = Record.Exception(() => perm.SetPermissions(true, true, true, true));
            var after3 = perm.UpdatedAt;

            // Assert
            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);

            Assert.NotNull(after1);
            Assert.NotNull(after2);
            Assert.NotNull(after3);

            Assert.True(after2 >= after1,
                "UpdatedAt deve ser atualizado ou avançar a cada mutação (SetScreen após SetRole).");
            Assert.True(after3 >= after2,
                "UpdatedAt deve avançar novamente após SetPermissions.");
        }
    }
}

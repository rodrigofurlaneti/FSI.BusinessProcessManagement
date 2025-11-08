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
        private static DateTime MinWhenNull(DateTime? dt) => dt ?? DateTime.MinValue;

        // --------------------------------------------------------------------
        // 1) CONTRATO / ESTRUTURA
        // --------------------------------------------------------------------

        [Fact]
        public void Class_Must_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(RoleScreenPermission);

            Assert.NotNull(t);
            Assert.True(t.IsSealed, "RoleScreenPermission deve continuar sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(RoleScreenPermission);

            // RoleId
            var roleId = t.GetProperty("RoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(roleId);
            Assert.Equal(typeof(long), roleId!.PropertyType);
            Assert.True(roleId.GetGetMethod()!.IsPublic);
            Assert.Null(roleId.GetSetMethod());
            Assert.True(roleId.GetSetMethod(true)!.IsPrivate);

            // ScreenId
            var screenId = t.GetProperty("ScreenId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(screenId);
            Assert.Equal(typeof(long), screenId!.PropertyType);
            Assert.True(screenId.GetGetMethod()!.IsPublic);
            Assert.Null(screenId.GetSetMethod());
            Assert.True(screenId.GetSetMethod(true)!.IsPrivate);

            // CanView
            var canView = t.GetProperty("CanView", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canView);
            Assert.Equal(typeof(bool), canView!.PropertyType);
            Assert.True(canView.GetGetMethod()!.IsPublic);
            Assert.Null(canView.GetSetMethod());
            Assert.True(canView.GetSetMethod(true)!.IsPrivate);

            // CanCreate
            var canCreate = t.GetProperty("CanCreate", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canCreate);
            Assert.Equal(typeof(bool), canCreate!.PropertyType);
            Assert.True(canCreate.GetGetMethod()!.IsPublic);
            Assert.Null(canCreate.GetSetMethod());
            Assert.True(canCreate.GetSetMethod(true)!.IsPrivate);

            // CanEdit
            var canEdit = t.GetProperty("CanEdit", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canEdit);
            Assert.Equal(typeof(bool), canEdit!.PropertyType);
            Assert.True(canEdit.GetGetMethod()!.IsPublic);
            Assert.Null(canEdit.GetSetMethod());
            Assert.True(canEdit.GetSetMethod(true)!.IsPrivate);

            // CanDelete
            var canDelete = t.GetProperty("CanDelete", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(canDelete);
            Assert.Equal(typeof(bool), canDelete!.PropertyType);
            Assert.True(canDelete.GetGetMethod()!.IsPublic);
            Assert.Null(canDelete.GetSetMethod());
            Assert.True(canDelete.GetSetMethod(true)!.IsPrivate);
        }

        [Fact]
        public void Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(RoleScreenPermission);

            // Ctor privado sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "Ctor vazio deve permanecer private para EF.");

            // Ctor público esperado: (long roleId, long screenId, bool canView, bool canCreate, bool canEdit, bool canDelete)
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
        public void Public_Methods_Signatures_Must_Remain()
        {
            var t = typeof(RoleScreenPermission);

            var setRole = t.GetMethod("SetRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setRole);
            Assert.Equal(typeof(long), setRole!.GetParameters()[0].ParameterType);

            var setScreen = t.GetMethod("SetScreen", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setScreen);
            Assert.Equal(typeof(long), setScreen!.GetParameters()[0].ParameterType);

            var setPermissions = t.GetMethod("SetPermissions", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setPermissions);
            var p = setPermissions!.GetParameters();
            Assert.Equal(4, p.Length);
            Assert.True(p.All(x => x.ParameterType == typeof(bool)));
        }

        // --------------------------------------------------------------------
        // 2) CONSTRUTOR / ESTADO INICIAL
        // --------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArgs_ShouldInitialize_AllFields_AndTouch()
        {
            var beforeUtc = DateTime.UtcNow;

            var perm = new RoleScreenPermission(
                roleId: 10,
                screenId: 20,
                canView: true,
                canCreate: false,
                canEdit: true,
                canDelete: false
            );

            var afterUtc = DateTime.UtcNow;

            Assert.Equal(10, perm.RoleId);
            Assert.Equal(20, perm.ScreenId);
            Assert.True(perm.CanView);
            Assert.False(perm.CanCreate);
            Assert.True(perm.CanEdit);
            Assert.False(perm.CanDelete);

            // Construtor chama SetRole/SetScreen/SetPermissions => Touch()
            Assert.NotNull(perm.UpdatedAt);
            Assert.True(perm.UpdatedAt!.Value >= beforeUtc && perm.UpdatedAt!.Value <= afterUtc);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidRole_ShouldThrow(long invalidRole)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new RoleScreenPermission(invalidRole, 1, true, false, false, false)
            );
            Assert.Equal("Invalid role.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidScreen_ShouldThrow(long invalidScreen)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new RoleScreenPermission(1, invalidScreen, true, false, false, false)
            );
            Assert.Equal("Invalid screen.", ex.Message);
        }

        // --------------------------------------------------------------------
        // 3) SetRole
        // --------------------------------------------------------------------

        [Fact]
        public void SetRole_WithValidValue_ShouldAssign_AndTouch()
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);
            var before = MinWhenNull(perm.UpdatedAt);

            perm.SetRole(99);
            var after = MinWhenNull(perm.UpdatedAt);

            Assert.Equal(99, perm.RoleId);
            Assert.True(after > before, "SetRole deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetRole_WithInvalidValue_ShouldThrow(long invalidRole)
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);
            var ex = Assert.Throws<DomainException>(() => perm.SetRole(invalidRole));
            Assert.Equal("Invalid role.", ex.Message);
        }

        // --------------------------------------------------------------------
        // 4) SetScreen
        // --------------------------------------------------------------------

        [Fact]
        public void SetScreen_WithValidValue_ShouldAssign_AndTouch()
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);
            var before = MinWhenNull(perm.UpdatedAt);

            perm.SetScreen(77);
            var after = MinWhenNull(perm.UpdatedAt);

            Assert.Equal(77, perm.ScreenId);
            Assert.True(after > before, "SetScreen deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetScreen_WithInvalidValue_ShouldThrow(long invalidScreen)
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);
            var ex = Assert.Throws<DomainException>(() => perm.SetScreen(invalidScreen));
            Assert.Equal("Invalid screen.", ex.Message);
        }

        // --------------------------------------------------------------------
        // 5) SetPermissions
        // --------------------------------------------------------------------

        [Theory]
        [InlineData(false, false, false, false)]
        [InlineData(true, false, false, false)]
        [InlineData(false, true, false, false)]
        [InlineData(false, false, true, false)]
        [InlineData(false, false, false, true)]
        [InlineData(true, true, true, true)]
        [InlineData(true, false, true, false)]
        public void SetPermissions_ShouldAssignAllFlags_AndTouch(
            bool v, bool c, bool e, bool d)
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);
            var before = MinWhenNull(perm.UpdatedAt);

            perm.SetPermissions(v, c, e, d);
            var after = MinWhenNull(perm.UpdatedAt);

            Assert.Equal(v, perm.CanView);
            Assert.Equal(c, perm.CanCreate);
            Assert.Equal(e, perm.CanEdit);
            Assert.Equal(d, perm.CanDelete);

            Assert.True(after > before, "SetPermissions deve chamar Touch().");
        }

        // --------------------------------------------------------------------
        // 6) Mutation stability (ordem de toques)
        // --------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldAlwaysAdvance_UpdatedAt_InOrder()
        {
            var perm = new RoleScreenPermission(1, 2, false, false, false, false);

            var t0 = MinWhenNull(perm.UpdatedAt);

            perm.SetRole(10);
            var t1 = MinWhenNull(perm.UpdatedAt);
            Assert.True(t1 > t0, "SetRole deve avançar UpdatedAt.");

            perm.SetScreen(20);
            var t2 = MinWhenNull(perm.UpdatedAt);
            Assert.True(t2 > t1, "SetScreen deve avançar UpdatedAt.");

            perm.SetPermissions(true, true, false, true);
            var t3 = MinWhenNull(perm.UpdatedAt);
            Assert.True(t3 > t2, "SetPermissions deve avançar UpdatedAt.");
        }
    }
}

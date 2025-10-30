using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class UserTests
    {
        // -------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -------------------------------------------------------------------------

        [Fact]
        public void User_Class_MustBeSealed_And_Inherit_BaseEntity()
        {
            var t = typeof(User);

            Assert.True(t.IsSealed, "User deve continuar sendo sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void User_Properties_MustExist_WithExpectedTypes_And_PrivateSetters()
        {
            var t = typeof(User);

            // DepartmentId
            var depIdProp = t.GetProperty("DepartmentId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(depIdProp);
            Assert.Equal(typeof(long?), depIdProp!.PropertyType);
            Assert.NotNull(depIdProp.GetGetMethod());
            Assert.Null(depIdProp.GetSetMethod());
            Assert.True(depIdProp.GetSetMethod(true)!.IsPrivate,
                "DepartmentId deve manter 'private set;'.");

            // Username
            var usernameProp = t.GetProperty("Username", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(usernameProp);
            Assert.Equal(typeof(string), usernameProp!.PropertyType);
            Assert.NotNull(usernameProp.GetGetMethod());
            Assert.Null(usernameProp.GetSetMethod());
            Assert.True(usernameProp.GetSetMethod(true)!.IsPrivate,
                "Username deve manter 'private set;'.");

            // PasswordHash
            var pwdProp = t.GetProperty("PasswordHash", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(pwdProp);
            Assert.Equal(typeof(string), pwdProp!.PropertyType);
            Assert.NotNull(pwdProp.GetGetMethod());
            Assert.Null(pwdProp.GetSetMethod());
            Assert.True(pwdProp.GetSetMethod(true)!.IsPrivate,
                "PasswordHash deve manter 'private set;'.");

            // Email
            var emailProp = t.GetProperty("Email", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(emailProp);
            Assert.Equal(typeof(string), emailProp!.PropertyType); // string? em runtime = System.String
            Assert.NotNull(emailProp.GetGetMethod());
            Assert.Null(emailProp.GetSetMethod());
            Assert.True(emailProp.GetSetMethod(true)!.IsPrivate,
                "Email deve manter 'private set;'.");

            // IsActive
            var activeProp = t.GetProperty("IsActive", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(activeProp);
            Assert.Equal(typeof(bool), activeProp!.PropertyType);
            Assert.NotNull(activeProp.GetGetMethod());
            Assert.Null(activeProp.GetSetMethod());
            Assert.True(activeProp.GetSetMethod(true)!.IsPrivate,
                "IsActive deve manter 'private set;'.");

            // UserRoles (IReadOnlyCollection<UserRole>)
            var rolesProp = t.GetProperty("UserRoles", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(rolesProp);
            Assert.Equal(typeof(IReadOnlyCollection<UserRole>), rolesProp!.PropertyType);
            Assert.NotNull(rolesProp.GetGetMethod());
            Assert.Null(rolesProp.GetSetMethod()); // não queremos setter público NEM privado
        }

        [Fact]
        public void User_MustHave_PrivateParameterlessCtor_And_PublicMainCtor()
        {
            var t = typeof(User);

            // ctor privado sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private (requisito EF).");

            // ctor público (string username, string passwordHash, long? departmentId, string? email, bool isActive)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 5
                        && p[0].ParameterType == typeof(string)   // username
                        && p[1].ParameterType == typeof(string)   // passwordHash
                        && p[2].ParameterType == typeof(long?)    // departmentId
                        && p[3].ParameterType == typeof(string)   // email (string?)
                        && p[4].ParameterType == typeof(bool);    // isActive
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void User_PublicMethods_MustExist_WithExpectedSignatures()
        {
            var t = typeof(User);

            // SetUsername(string username)
            var m1 = t.GetMethod("SetUsername", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m1);
            Assert.Single(m1!.GetParameters());
            Assert.Equal(typeof(string), m1.GetParameters()[0].ParameterType);

            // SetPasswordHash(string passwordHash)
            var m2 = t.GetMethod("SetPasswordHash", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m2);
            Assert.Single(m2!.GetParameters());
            Assert.Equal(typeof(string), m2.GetParameters()[0].ParameterType);

            // SetEmail(string? email)
            var m3 = t.GetMethod("SetEmail", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m3);
            Assert.Single(m3!.GetParameters());
            Assert.Equal(typeof(string), m3.GetParameters()[0].ParameterType);

            // SetDepartment(long? departmentId)
            var m4 = t.GetMethod("SetDepartment", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m4);
            Assert.Single(m4!.GetParameters());
            Assert.Equal(typeof(long?), m4.GetParameters()[0].ParameterType);

            // Activate()
            var m5 = t.GetMethod("Activate", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m5);
            Assert.Empty(m5!.GetParameters());

            // Deactivate()
            var m6 = t.GetMethod("Deactivate", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m6);
            Assert.Empty(m6!.GetParameters());

            // AddRole(Role role)
            var m7 = t.GetMethod("AddRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m7);
            Assert.Single(m7!.GetParameters());
            Assert.Equal(typeof(Role), m7.GetParameters()[0].ParameterType);

            // RemoveRole(long roleId)
            var m8 = t.GetMethod("RemoveRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(m8);
            Assert.Single(m8!.GetParameters());
            Assert.Equal(typeof(long), m8.GetParameters()[0].ParameterType);
        }

        // -------------------------------------------------------------------------
        // 2. CONSTRUTOR / INICIALIZAÇÃO
        // -------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_AllFields_Trim_AndTouch()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var user = new User(
                username: "   r.furlaneti   ",
                passwordHash: "HASH123",
                departmentId: 42,
                email: "   rodrigo@example.com   ",
                isActive: true
            );

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal("r.furlaneti", user.Username);
            Assert.Equal("HASH123", user.PasswordHash);
            Assert.Equal(42, user.DepartmentId);
            Assert.Equal("rodrigo@example.com", user.Email);
            Assert.True(user.IsActive);

            // Coleção de roles deve existir e estar vazia
            Assert.NotNull(user.UserRoles);
            Assert.Empty(user.UserRoles);

            // UpdatedAt deve ter sido preenchido, porque SetUsername / SetPasswordHash / SetDepartment / SetEmail chamam Touch()
            Assert.NotNull(user.UpdatedAt);
            Assert.InRange(user.UpdatedAt!.Value, before, after);

            // CreatedAt vem do BaseEntity
            Assert.True(user.CreatedAt <= DateTime.Now && user.CreatedAt > DateTime.Now.AddMinutes(-1));
        }

        [Fact]
        public void Constructor_WithMinimalData_ShouldStillWork()
        {
            var user = new User(
                username: "testuser",
                passwordHash: "hash",
                departmentId: null,
                email: null,
                isActive: false
            );

            Assert.Equal("testuser", user.Username);
            Assert.Equal("hash", user.PasswordHash);
            Assert.Null(user.DepartmentId);
            Assert.Null(user.Email);
            Assert.False(user.IsActive);
            Assert.NotNull(user.UserRoles);
        }

        // -------------------------------------------------------------------------
        // 3. SetUsername
        // -------------------------------------------------------------------------

        [Fact]
        public void SetUsername_WithValidValue_ShouldTrimAndTouch()
        {
            var u = new User("base", "hash");
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetUsername("   novo_user   ");

            Assert.Equal("novo_user", u.Username);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before, "SetUsername deve chamar Touch().");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetUsername_Invalid_ShouldThrowDomainException(string invalid)
        {
            var u = new User("valid", "hash");

            var ex = Assert.Throws<DomainException>(() => u.SetUsername(invalid));
            Assert.Equal("Username is required.", ex.Message);
        }

        [Fact]
        public void SetUsername_LongerThan100Chars_ShouldThrowDomainException()
        {
            var u = new User("valid", "hash");

            string tooLong = new string('X', 101);
            var ex = Assert.Throws<DomainException>(() => u.SetUsername(tooLong));
            Assert.Equal("Username too long (max 100).", ex.Message);
        }

        // -------------------------------------------------------------------------
        // 4. SetPasswordHash
        // -------------------------------------------------------------------------

        [Fact]
        public void SetPasswordHash_WithValidValue_ShouldUpdateHash_AndTouch()
        {
            var u = new User("user", "oldHash");
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetPasswordHash("NewHashValue");

            Assert.Equal("NewHashValue", u.PasswordHash);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetPasswordHash_Invalid_ShouldThrowDomainException(string badHash)
        {
            var u = new User("user", "valid");

            var ex = Assert.Throws<DomainException>(() => u.SetPasswordHash(badHash));
            Assert.Equal("PasswordHash is required (store hashes only).", ex.Message);
        }

        [Fact]
        public void SetPasswordHash_LongerThan255Chars_ShouldThrowDomainException()
        {
            var u = new User("user", "valid");
            string tooLong = new string('A', 256);

            var ex = Assert.Throws<DomainException>(() => u.SetPasswordHash(tooLong));
            Assert.Equal("PasswordHash too long (max 255).", ex.Message);
        }

        // -------------------------------------------------------------------------
        // 5. SetEmail
        // -------------------------------------------------------------------------

        [Fact]
        public void SetEmail_WithValidValue_ShouldTrim_Assign_AndTouch()
        {
            var u = new User("user", "hash");
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetEmail("   mail@example.com   ");

            Assert.Equal("mail@example.com", u.Email);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Fact]
        public void SetEmail_WithNull_ShouldSetNull_AndTouch()
        {
            var u = new User("user", "hash", email: "mail@x.com");
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetEmail(null);

            Assert.Null(u.Email);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Fact]
        public void SetEmail_WithWhitespace_ShouldThrowDomainException_BecauseEmailIsTrimmedFirst_AndMissingAtSymbol()
        {
            // "   " -> trimmed = "" -> does not contain '@' -> "Email format invalid."
            var u = new User("user", "hash");

            var ex = Assert.Throws<DomainException>(() => u.SetEmail("      "));
            Assert.Equal("Email format invalid.", ex.Message);
        }

        [Fact]
        public void SetEmail_WithoutAtSymbol_ShouldThrowDomainException()
        {
            var u = new User("user", "hash");

            var ex = Assert.Throws<DomainException>(() => u.SetEmail("email.invalido.sem.arroba"));
            Assert.Equal("Email format invalid.", ex.Message);
        }

        [Fact]
        public void SetEmail_LongerThan200Chars_ShouldThrowDomainException()
        {
            var u = new User("user", "hash");

            string local = new string('a', 201);
            var ex = Assert.Throws<DomainException>(() => u.SetEmail(local + "@x.com"));
            Assert.Equal("Email too long (max 200).", ex.Message);
        }

        // -------------------------------------------------------------------------
        // 6. SetDepartment
        // -------------------------------------------------------------------------

        [Fact]
        public void SetDepartment_ShouldUpdateDepartmentId_AndTouch()
        {
            var u = new User("user", "hash", departmentId: 10);
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetDepartment(99);

            Assert.Equal(99, u.DepartmentId);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Fact]
        public void SetDepartment_ShouldAllowNull_AndTouch()
        {
            var u = new User("user", "hash", departmentId: 10);
            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.SetDepartment(null);

            Assert.Null(u.DepartmentId);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        // -------------------------------------------------------------------------
        // 7. Activate / Deactivate
        // -------------------------------------------------------------------------

        [Fact]
        public void Deactivate_ShouldSetIsActiveFalse_AndTouch_IfCurrentlyActive()
        {
            var u = new User("user", "hash", isActive: true);
            Assert.True(u.IsActive);

            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.Deactivate();

            Assert.False(u.IsActive);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Fact]
        public void Deactivate_WhenAlreadyInactive_ShouldDoNothing_AndNotThrow()
        {
            var u = new User("user", "hash", isActive: false);

            var beforeActive = u.IsActive;
            var beforeUpdatedAt = u.UpdatedAt;

            var ex = Record.Exception(() => u.Deactivate());

            Assert.Null(ex);
            Assert.False(u.IsActive); // continua false
            // pode ou não ter mexido em UpdatedAt, porque método retorna cedo
            Assert.Equal(beforeActive, u.IsActive);
            Assert.Equal(beforeUpdatedAt, u.UpdatedAt);
        }

        [Fact]
        public void Activate_ShouldSetIsActiveTrue_AndTouch_IfCurrentlyInactive()
        {
            var u = new User("user", "hash", isActive: false);
            Assert.False(u.IsActive);

            var before = u.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            u.Activate();

            Assert.True(u.IsActive);
            Assert.NotNull(u.UpdatedAt);
            Assert.True(u.UpdatedAt >= before);
        }

        [Fact]
        public void Activate_WhenAlreadyActive_ShouldDoNothing_AndNotThrow()
        {
            var u = new User("user", "hash", isActive: true);

            var beforeActive = u.IsActive;
            var beforeUpdatedAt = u.UpdatedAt;

            var ex = Record.Exception(() => u.Activate());

            Assert.Null(ex);
            Assert.True(u.IsActive); // continua true
            Assert.Equal(beforeActive, u.IsActive);
            Assert.Equal(beforeUpdatedAt, u.UpdatedAt);
        }

        // -------------------------------------------------------------------------
        // 8. AddRole / RemoveRole
        // -------------------------------------------------------------------------

        [Fact]
        public void AddRole_WithValidRole_ShouldAddNewUserRole_AndTouch()
        {
            // Arrange
            var user = new User("user", "hash", isActive: true);
            var role = new Role("Administrador");

            // Vamos simular IDs já persistidos
            // como Id tem protected set, não dá pra setar direto aqui de fora.
            // Mas podemos validar comportamento sem depender do valor real do Id do usuário/role,
            // apenas que 1 item foi adicionado e referencia o Role informado.

            var beforeCount = user.UserRoles.Count;
            var beforeUpdatedAt = user.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            var ex = Record.Exception(() => user.AddRole(role));

            // Assert
            Assert.Null(ex);
            Assert.Equal(beforeCount + 1, user.UserRoles.Count);

            // Verifica se existe um UserRole cujo RoleId == role.Id
            // (role.Id por padrão começa em 0 até persistir em DB. Isso é OK para o teste.)
            Assert.Contains(user.UserRoles, ur => ur.RoleId == role.Id);

            Assert.NotNull(user.UpdatedAt);
            Assert.True(user.UpdatedAt >= beforeUpdatedAt,
                "AddRole deve chamar Touch().");
        }

        [Fact]
        public void AddRole_WithNullRole_ShouldThrowArgumentNullException()
        {
            var user = new User("user", "hash");

            Assert.Throws<ArgumentNullException>(() => user.AddRole(null));
        }

        [Fact]
        public void AddRole_WhenRoleAlreadyAssigned_ShouldThrowDomainException()
        {
            // Arrange
            var user = new User("user", "hash");
            var role = new Role("Operador");

            user.AddRole(role);

            // Act
            var ex = Assert.Throws<DomainException>(() => user.AddRole(role));

            // Assert
            Assert.Equal("User already has this role.", ex.Message);
        }

        [Fact]
        public void RemoveRole_ShouldRemoveExistingRole_AndTouch()
        {
            // Arrange
            var user = new User("user", "hash");
            var role = new Role("Supervisor");

            user.AddRole(role);

            var beforeCount = user.UserRoles.Count;
            var beforeUpdatedAt = user.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            var ex = Record.Exception(() => user.RemoveRole(role.Id));

            // Assert
            Assert.Null(ex);
            Assert.Equal(beforeCount - 1, user.UserRoles.Count);

            Assert.NotNull(user.UpdatedAt);
            Assert.True(user.UpdatedAt >= beforeUpdatedAt,
                "RemoveRole deve chamar Touch().");
        }

        [Fact]
        public void RemoveRole_NotAssigned_ShouldThrowDomainException()
        {
            var user = new User("user", "hash");
            var ex = Assert.Throws<DomainException>(() => user.RemoveRole(999));
            Assert.Equal("Role not assigned to user.", ex.Message);
        }

        // -------------------------------------------------------------------------
        // 9. Mutations overall (sanity of Touch / monotonic UpdatedAt)
        // -------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldNotThrow_And_ShouldAdvanceUpdatedAt()
        {
            var user = new User("user", "hash", isActive: true);

            var ex1 = Record.Exception(() => user.SetUsername("u1"));
            var ts1 = user.UpdatedAt;

            var ex2 = Record.Exception(() => user.SetPasswordHash("h2"));
            var ts2 = user.UpdatedAt;

            var ex3 = Record.Exception(() => user.SetEmail("mail@test.com"));
            var ts3 = user.UpdatedAt;

            var ex4 = Record.Exception(() => user.SetDepartment(22));
            var ts4 = user.UpdatedAt;

            var ex5 = Record.Exception(() => user.Deactivate());
            var ts5 = user.UpdatedAt;

            var ex6 = Record.Exception(() => user.Activate());
            var ts6 = user.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
            Assert.Null(ex5);
            Assert.Null(ex6);

            Assert.NotNull(ts1);
            Assert.NotNull(ts2);
            Assert.NotNull(ts3);
            Assert.NotNull(ts4);
            Assert.NotNull(ts5);
            Assert.NotNull(ts6);

            Assert.True(ts2 >= ts1);
            Assert.True(ts3 >= ts2);
            Assert.True(ts4 >= ts3);
            Assert.True(ts5 >= ts4);
            Assert.True(ts6 >= ts5);
        }
    }
}

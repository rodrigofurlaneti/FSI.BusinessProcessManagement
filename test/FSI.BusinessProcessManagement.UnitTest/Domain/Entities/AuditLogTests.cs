using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class AuditLogTests
    {
        // ------------------------------------------------------------------------------------------------
        // 1. TESTES DE CONTRATO / INTEGRIDADE ESTRUTURAL
        //    -> Se alguém renomear classe, propriedade, tipo, visibilidade do set, etc., o teste quebra.
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void AuditLog_Class_Must_Exist_And_Be_Sealed_And_Inherit_BaseEntity()
        {
            // Arrange
            var auditLogType = typeof(AuditLog);

            // Assert
            Assert.NotNull(auditLogType); // classe ainda existe

            Assert.True(auditLogType.IsSealed,
                "AuditLog deve continuar sendo sealed. Se precisar mudar, atualize este teste.");

            Assert.True(auditLogType.BaseType == typeof(BaseEntity),
                "AuditLog deve continuar herdando de BaseEntity. Se mudar a herança, atualize este teste.");
        }

        [Fact]
        public void AuditLog_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(AuditLog);

            // UserId
            var userIdProp = t.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userIdProp);
            Assert.Equal(typeof(long?), userIdProp!.PropertyType);
            Assert.NotNull(userIdProp.GetGetMethod()); // tem get público
            Assert.Null(userIdProp.GetSetMethod());    // NÃO pode ter set público
            Assert.NotNull(userIdProp.GetSetMethod(true)); // mas tem set private/protected
            Assert.True(userIdProp.GetSetMethod(true)!.IsPrivate,
                "UserId deve continuar com set privado (private set;).");

            // ScreenId
            var screenIdProp = t.GetProperty("ScreenId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(screenIdProp);
            Assert.Equal(typeof(long?), screenIdProp!.PropertyType);
            Assert.NotNull(screenIdProp.GetGetMethod());
            Assert.Null(screenIdProp.GetSetMethod());
            Assert.True(screenIdProp.GetSetMethod(true)!.IsPrivate,
                "ScreenId deve continuar com set privado (private set;).");

            // ActionType
            var actionTypeProp = t.GetProperty("ActionType", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(actionTypeProp);
            Assert.Equal(typeof(string), actionTypeProp!.PropertyType);
            Assert.NotNull(actionTypeProp.GetGetMethod());
            Assert.Null(actionTypeProp.GetSetMethod());
            Assert.True(actionTypeProp.GetSetMethod(true)!.IsPrivate,
                "ActionType deve continuar com set privado (private set;).");

            // ActionTimestamp
            var actionTimestampProp = t.GetProperty("ActionTimestamp", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(actionTimestampProp);
            Assert.Equal(typeof(DateTime), actionTimestampProp!.PropertyType);
            Assert.NotNull(actionTimestampProp.GetGetMethod());
            Assert.Null(actionTimestampProp.GetSetMethod());
            Assert.True(actionTimestampProp.GetSetMethod(true)!.IsPrivate,
                "ActionTimestamp deve continuar com set privado (private set;).");

            // AdditionalInfo
            var additionalProp = t.GetProperty("AdditionalInfo", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(additionalProp);
            Assert.Equal(typeof(string), additionalProp!.PropertyType); // string? => em runtime é System.String
            Assert.NotNull(additionalProp.GetGetMethod());
            Assert.Null(additionalProp.GetSetMethod());
            Assert.True(additionalProp.GetSetMethod(true)!.IsPrivate,
                "AdditionalInfo deve continuar com set privado (private set;).");
        }

        [Fact]
        public void AuditLog_Must_Have_Private_Parameterless_Constructor_And_Public_Application_Constructor()
        {
            var t = typeof(AuditLog);

            // ctor privado sem parâmetros (ORM / reflection / etc.)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor sem parâmetros deve continuar private. Se mudar, atualize o teste.");

            // ctor público esperado pela Application
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 4
                        && p[0].ParameterType == typeof(string)   // actionType
                        && p[1].ParameterType == typeof(long?)    // userId
                        && p[2].ParameterType == typeof(long?)    // screenId
                        && p[3].ParameterType == typeof(string);  // additionalInfo (string?)
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void AuditLog_Public_Methods_Must_Exist_And_Signatures_Must_Remain()
        {
            var t = typeof(AuditLog);

            // SetUser(long? userId)
            var setUser = t.GetMethod("SetUser", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setUser);
            var pSetUser = setUser!.GetParameters();
            Assert.Single(pSetUser);
            Assert.Equal(typeof(long?), pSetUser[0].ParameterType);

            // SetScreen(long? screenId)
            var setScreen = t.GetMethod("SetScreen", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setScreen);
            var pSetScreen = setScreen!.GetParameters();
            Assert.Single(pSetScreen);
            Assert.Equal(typeof(long?), pSetScreen[0].ParameterType);

            // SetActionType(string actionType)
            var setActionType = t.GetMethod("SetActionType", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setActionType);
            var pSetActionType = setActionType!.GetParameters();
            Assert.Single(pSetActionType);
            Assert.Equal(typeof(string), pSetActionType[0].ParameterType);

            // SetAdditionalInfo(string? info)
            var setAdditionalInfo = t.GetMethod("SetAdditionalInfo", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setAdditionalInfo);
            var pSetAdditionalInfo = setAdditionalInfo!.GetParameters();
            Assert.Single(pSetAdditionalInfo);
            Assert.Equal(typeof(string), pSetAdditionalInfo[0].ParameterType);

            // UpdateInfo(string? info) -> wrapper legacy
            var updateInfo = t.GetMethod("UpdateInfo", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(updateInfo);
            var pUpdateInfo = updateInfo!.GetParameters();
            Assert.Single(pUpdateInfo);
            Assert.Equal(typeof(string), pUpdateInfo[0].ParameterType);
        }

        // ------------------------------------------------------------------------------------------------
        // 2. TESTES DE CONSTRUTOR
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_Valid_MinimalParameters_ShouldInitialize_ActionType_And_Timestamps()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var log = new AuditLog(actionType: "CREATE");

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal("CREATE", log.ActionType);
            Assert.Null(log.UserId);
            Assert.Null(log.ScreenId);
            Assert.Null(log.AdditionalInfo);
            Assert.True(log.ActionTimestamp >= before && log.ActionTimestamp <= after,
                "ActionTimestamp deve ser definido no construtor com UtcNow.");
        }

        [Fact]
        public void Constructor_Valid_AllParameters_ShouldInitialize_AllFields()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var log = new AuditLog(
                actionType: "UPDATE",
                userId: 10,
                screenId: 22,
                additionalInfo: " Detalhes extras "
            );

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal("UPDATE", log.ActionType);
            Assert.Equal(10, log.UserId);
            Assert.Equal(22, log.ScreenId);
            Assert.Equal("Detalhes extras", log.AdditionalInfo); // Trim aplicado
            Assert.True(log.ActionTimestamp >= before && log.ActionTimestamp <= after);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_Invalid_ActionType_ShouldThrowDomainException(string invalidActionType)
        {
            Assert.Throws<DomainException>(() =>
                new AuditLog(invalidActionType)
            );
        }

        [Fact]
        public void Constructor_ActionType_TooLong_ShouldThrowDomainException()
        {
            // 61 chars
            string tooLong = new string('A', 61);

            Assert.Throws<DomainException>(() =>
                new AuditLog(tooLong)
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void Constructor_Invalid_UserId_ShouldThrowDomainException(long invalidUser)
        {
            Assert.Throws<DomainException>(() =>
                new AuditLog("SOMETHING", userId: invalidUser)
            );
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void Constructor_Invalid_ScreenId_ShouldThrowDomainException(long invalidScreen)
        {
            Assert.Throws<DomainException>(() =>
                new AuditLog("SOMETHING", userId: 1, screenId: invalidScreen)
            );
        }

        // ------------------------------------------------------------------------------------------------
        // 3. TESTES DOS SETTERS PÚBLICOS (REGRAS DE DOMÍNIO)
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetUser_WithValidUserId_ShouldUpdateUserId()
        {
            var log = new AuditLog("LOGIN");

            log.SetUser(123);

            Assert.Equal(123, log.UserId);
        }

        [Fact]
        public void SetUser_WithNull_ShouldAllowNull()
        {
            var log = new AuditLog("LOGIN", userId: 10);

            log.SetUser(null);

            Assert.Null(log.UserId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void SetUser_WithInvalidUserId_ShouldThrowDomainException(long invalidUser)
        {
            var log = new AuditLog("LOGIN");

            var ex = Assert.Throws<DomainException>(() => log.SetUser(invalidUser));
            Assert.Equal("Invalid UserId.", ex.Message);
        }

        [Fact]
        public void SetScreen_WithValidScreenId_ShouldUpdateScreenId()
        {
            var log = new AuditLog("OPEN_SCREEN");

            log.SetScreen(999);

            Assert.Equal(999, log.ScreenId);
        }

        [Fact]
        public void SetScreen_WithNull_ShouldAllowNull()
        {
            var log = new AuditLog("OPEN_SCREEN", screenId: 2);

            log.SetScreen(null);

            Assert.Null(log.ScreenId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void SetScreen_WithInvalidScreenId_ShouldThrowDomainException(long invalidScreen)
        {
            var log = new AuditLog("OPEN_SCREEN");

            var ex = Assert.Throws<DomainException>(() => log.SetScreen(invalidScreen));
            Assert.Equal("Invalid ScreenId.", ex.Message);
        }

        [Fact]
        public void SetActionType_WithValidValue_ShouldTrimAndUpdate_AndRefreshTimestamp()
        {
            var log = new AuditLog("X");

            var before = log.ActionTimestamp;
            System.Threading.Thread.Sleep(5); // garante mudança perceptível
            log.SetActionType("  DELETE   ");

            Assert.Equal("DELETE", log.ActionType);
            Assert.True(log.ActionTimestamp > before,
                "Ao chamar SetActionType o ActionTimestamp deve ser atualizado com UtcNow.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void SetActionType_WithNullOrWhitespace_ShouldThrowDomainException(string invalid)
        {
            var log = new AuditLog("INIT");

            var ex = Assert.Throws<DomainException>(() => log.SetActionType(invalid));
            Assert.Equal("ActionType is required.", ex.Message);
        }

        [Fact]
        public void SetActionType_WithMoreThan60Chars_ShouldThrowDomainException()
        {
            var log = new AuditLog("INIT");

            string tooLong = new string('B', 61);

            var ex = Assert.Throws<DomainException>(() => log.SetActionType(tooLong));
            Assert.Equal("ActionType too long (max 60).", ex.Message);
        }

        [Fact]
        public void SetAdditionalInfo_WithValidString_ShouldTrimAndSet_AndRefreshTimestamp()
        {
            var log = new AuditLog("ANY");

            var before = log.ActionTimestamp;
            System.Threading.Thread.Sleep(5);

            log.SetAdditionalInfo("   detalhe técnico XYZ   ");

            Assert.Equal("detalhe técnico XYZ", log.AdditionalInfo);
            Assert.True(log.ActionTimestamp > before,
                "Ao chamar SetAdditionalInfo o ActionTimestamp deve ser atualizado com UtcNow.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetAdditionalInfo_WithNullOrWhitespace_ShouldBecomeNull_AndRefreshTimestamp(string? info)
        {
            var log = new AuditLog("ANY");

            var before = log.ActionTimestamp;
            System.Threading.Thread.Sleep(5);

            log.SetAdditionalInfo(info);

            Assert.Null(log.AdditionalInfo);
            Assert.True(log.ActionTimestamp > before,
                "Mesmo limpando info, ActionTimestamp deve ser atualizado.");
        }

        [Fact]
        public void UpdateInfo_MustCall_SetAdditionalInfo_SameBehavior()
        {
            var log = new AuditLog("ANY");

            log.UpdateInfo("   pacote JSON enviado   ");
            Assert.Equal("pacote JSON enviado", log.AdditionalInfo);

            log.UpdateInfo(null);
            Assert.Null(log.AdditionalInfo);
        }

        // ------------------------------------------------------------------------------------------------
        // 4. TESTE DE "TOUCH" IMPLÍCITO
        //    Aqui não sabemos a implementação de Touch(), mas sabemos que TODOS os setters chamam Touch().
        //    O objetivo é garantir que NENHUM desses métodos lance exceção por causa do Touch().
        //    Se Touch() passar a exigir algo novo, esse teste quebra e o dev vai ter que ajustar o domínio e os testes.
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void All_PublicMutationMethods_ShouldNotThrow_WhenCalledWithValidData()
        {
            var log = new AuditLog("START", userId: 1, screenId: 2, additionalInfo: "ok");

            var ex1 = Record.Exception(() => log.SetUser(2));
            var ex2 = Record.Exception(() => log.SetScreen(3));
            var ex3 = Record.Exception(() => log.SetActionType("MOVE"));
            var ex4 = Record.Exception(() => log.SetAdditionalInfo("alguma info nova"));
            var ex5 = Record.Exception(() => log.UpdateInfo("legacy caminho feliz"));

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
            Assert.Null(ex5);
        }
    }
}

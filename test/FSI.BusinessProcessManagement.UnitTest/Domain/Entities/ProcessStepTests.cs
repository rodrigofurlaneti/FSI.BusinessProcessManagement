using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class ProcessStepTests
    {
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void ProcessStep_Class_Must_Exist_And_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(ProcessStep);

            Assert.NotNull(t);

            // A classe deve continuar sealed
            Assert.True(t.IsSealed,
                "ProcessStep deve continuar sendo sealed. Se mudar, atualize este teste.");

            // E deve continuar herdando BaseEntity
            Assert.True(t.BaseType == typeof(BaseEntity),
                "ProcessStep deve continuar herdando BaseEntity. Se mudar a herança, atualize este teste.");
        }

        [Fact]
        public void ProcessStep_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(ProcessStep);

            // ProcessId
            var processIdProp = t.GetProperty("ProcessId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(processIdProp);
            Assert.Equal(typeof(long), processIdProp!.PropertyType);

            var processIdGetter = processIdProp.GetGetMethod();
            Assert.NotNull(processIdGetter);           // precisa ter get público
            Assert.True(processIdGetter!.IsPublic);

            // setter público NÃO deve existir
            Assert.Null(processIdProp.GetSetMethod());

            // mas deve existir um setter privado (private set;)
            var processIdSetterPrivate = processIdProp.GetSetMethod(nonPublic: true);
            Assert.NotNull(processIdSetterPrivate);
            Assert.True(processIdSetterPrivate!.IsPrivate);

            // StepId (é só get => Id)
            var stepIdProp = t.GetProperty("StepId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepIdProp);
            Assert.Equal(typeof(long), stepIdProp!.PropertyType);

            var stepIdGetter = stepIdProp.GetGetMethod();
            Assert.NotNull(stepIdGetter);
            Assert.True(stepIdGetter!.IsPublic);

            // StepId não deve ter setter nem público nem privado
            Assert.Null(stepIdProp.GetSetMethod());
            Assert.Null(stepIdProp.GetSetMethod(true));

            // StepName
            var stepNameProp = t.GetProperty("StepName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepNameProp);
            Assert.Equal(typeof(string), stepNameProp!.PropertyType);

            var stepNameGetter = stepNameProp.GetGetMethod();
            Assert.NotNull(stepNameGetter);
            Assert.True(stepNameGetter!.IsPublic);

            Assert.Null(stepNameProp.GetSetMethod());

            var stepNameSetterPrivate = stepNameProp.GetSetMethod(true);
            Assert.NotNull(stepNameSetterPrivate);
            Assert.True(stepNameSetterPrivate!.IsPrivate);

            // StepOrder
            var stepOrderProp = t.GetProperty("StepOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepOrderProp);
            Assert.Equal(typeof(int), stepOrderProp!.PropertyType);

            var stepOrderGetter = stepOrderProp.GetGetMethod();
            Assert.NotNull(stepOrderGetter);
            Assert.True(stepOrderGetter!.IsPublic);

            Assert.Null(stepOrderProp.GetSetMethod());

            var stepOrderSetterPrivate = stepOrderProp.GetSetMethod(true);
            Assert.NotNull(stepOrderSetterPrivate);
            Assert.True(stepOrderSetterPrivate!.IsPrivate);

            // AssignedRoleId
            var assignedRoleProp = t.GetProperty("AssignedRoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(assignedRoleProp);
            Assert.Equal(typeof(long?), assignedRoleProp!.PropertyType);

            var assignedRoleGetter = assignedRoleProp.GetGetMethod();
            Assert.NotNull(assignedRoleGetter);
            Assert.True(assignedRoleGetter!.IsPublic);

            Assert.Null(assignedRoleProp.GetSetMethod());

            var assignedRoleSetterPrivate = assignedRoleProp.GetSetMethod(true);
            Assert.NotNull(assignedRoleSetterPrivate);
            Assert.True(assignedRoleSetterPrivate!.IsPrivate);
        }

        [Fact]
        public void ProcessStep_Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(ProcessStep);

            // ctor private sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private para o EF. Se mudar, atualize o teste.");

            // ctor público esperado:
            // (long processId, string stepName, int stepOrder, long? assignedRoleId)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 4
                           && p[0].ParameterType == typeof(long)
                           && p[1].ParameterType == typeof(string)
                           && p[2].ParameterType == typeof(int)
                           && p[3].ParameterType == typeof(long?);
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void ProcessStep_Public_Methods_Must_Exist_And_Signatures_Must_Remain()
        {
            var t = typeof(ProcessStep);

            // SetName(string name)
            var setName = t.GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setName);
            var pSetName = setName!.GetParameters();
            Assert.Single(pSetName);
            Assert.Equal(typeof(string), pSetName[0].ParameterType);

            // SetOrder(int order)
            var setOrder = t.GetMethod("SetOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setOrder);
            var pSetOrder = setOrder!.GetParameters();
            Assert.Single(pSetOrder);
            Assert.Equal(typeof(int), pSetOrder[0].ParameterType);

            // SetAssignedRole(long? roleId)
            var setAssignedRole = t.GetMethod("SetAssignedRole", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setAssignedRole);
            var pSetAssignedRole = setAssignedRole!.GetParameters();
            Assert.Single(pSetAssignedRole);
            Assert.Equal(typeof(long?), pSetAssignedRole[0].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR (REGRAS DE NEGÓCIO INICIAIS)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_AllFields_AndTrimName()
        {
            // Arrange
            var before = DateTime.UtcNow;

            // Act
            var step = new ProcessStep(
                processId: 10,
                stepName: "   Aprovar Solicitação   ",
                stepOrder: 2,
                assignedRoleId: 99
            );

            var after = DateTime.UtcNow;

            // Assert
            Assert.Equal(10, step.ProcessId);
            Assert.Equal("Aprovar Solicitação", step.StepName);
            Assert.Equal(2, step.StepOrder);
            Assert.Equal(99, step.AssignedRoleId);

            // StepId é um alias que devolve Id (Id nasce 0 em memória até persistir)
            Assert.Equal(step.Id, step.StepId);

            // UpdatedAt foi tocado pelos setters (Touch())
            Assert.NotNull(step.UpdatedAt);
            Assert.InRange(step.UpdatedAt!.Value, before, after);

            // CreatedAt vem de BaseEntity (DateTime.Now).
            // Aqui só garantimos que não é default e que está "recente".
            Assert.True(step.CreatedAt > DateTime.Now.AddMinutes(-1));
            Assert.True(step.CreatedAt <= DateTime.Now);
        }

        [Fact]
        public void Constructor_WithNullAssignedRole_ShouldStillBeValid()
        {
            var step = new ProcessStep(
                processId: 22,
                stepName: "Assinar Contrato",
                stepOrder: 0,
                assignedRoleId: null
            );

            Assert.Equal(22, step.ProcessId);
            Assert.Equal("Assinar Contrato", step.StepName);
            Assert.Equal(0, step.StepOrder);
            Assert.Null(step.AssignedRoleId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Constructor_InvalidProcessId_ShouldThrowDomainException(long invalidProcessId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(
                    processId: invalidProcessId,
                    stepName: "Validação",
                    stepOrder: 1,
                    assignedRoleId: null
                )
            );

            Assert.Equal("Invalid ProcessId.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidStepName_ShouldThrowDomainException(string badName)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(
                    processId: 10,
                    stepName: badName,
                    stepOrder: 1,
                    assignedRoleId: null
                )
            );

            Assert.Equal("Step name is required.", ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Constructor_InvalidStepOrder_ShouldThrowDomainException(int invalidOrder)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(
                    processId: 10,
                    stepName: "Validação",
                    stepOrder: invalidOrder,
                    assignedRoleId: null
                )
            );

            Assert.Equal("Step order must be >= 0.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidAssignedRoleId_ShouldThrowDomainException(long invalidRoleId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(
                    processId: 10,
                    stepName: "Validação",
                    stepOrder: 1,
                    assignedRoleId: invalidRoleId
                )
            );

            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetName
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetName_WithValidValue_ShouldTrim_And_Update_StepName_AndTouch()
        {
            // Arrange
            var step = new ProcessStep(10, "Inicial", 1, null);
            var before = step.UpdatedAt;

            Thread.Sleep(5); // só pra garantir diferença de tempo

            // Act
            step.SetName("   Revisar Documentos   ");

            // Assert
            Assert.Equal("Revisar Documentos", step.StepName);
            Assert.NotNull(step.UpdatedAt);
            Assert.True(step.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_WithNullOrWhitespace_ShouldThrowDomainException(string badName)
        {
            var step = new ProcessStep(10, "X", 1, null);

            var ex = Assert.Throws<DomainException>(() => step.SetName(badName));
            Assert.Equal("Step name is required.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetOrder
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetOrder_WithValidValue_ShouldUpdateStepOrder_AndTouch()
        {
            // Arrange
            var step = new ProcessStep(10, "Analisar Dados", 5, null);
            var before = step.UpdatedAt;

            Thread.Sleep(5);

            // Act
            step.SetOrder(9);

            // Assert
            Assert.Equal(9, step.StepOrder);
            Assert.NotNull(step.UpdatedAt);
            Assert.True(step.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetOrder_WithNegativeValue_ShouldThrowDomainException(int invalidOrder)
        {
            var step = new ProcessStep(10, "Analisar Dados", 5, null);

            var ex = Assert.Throws<DomainException>(() => step.SetOrder(invalidOrder));
            Assert.Equal("Step order must be >= 0.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 5. SetAssignedRole
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetAssignedRole_WithValidRoleId_ShouldUpdateAssignedRole_AndTouch()
        {
            // Arrange
            var step = new ProcessStep(10, "Avaliação Jurídica", 2, null);
            var before = step.UpdatedAt;

            Thread.Sleep(5);

            // Act
            step.SetAssignedRole(777);

            // Assert
            Assert.Equal(777, step.AssignedRoleId);
            Assert.NotNull(step.UpdatedAt);
            Assert.True(step.UpdatedAt >= before);
        }

        [Fact]
        public void SetAssignedRole_WithNull_ShouldClearAssignedRole_AndTouch()
        {
            // Arrange
            var step = new ProcessStep(10, "Avaliação Técnica", 2, 123);
            var before = step.UpdatedAt;

            Thread.Sleep(5);

            // Act
            step.SetAssignedRole(null);

            // Assert
            Assert.Null(step.AssignedRoleId);
            Assert.NotNull(step.UpdatedAt);
            Assert.True(step.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void SetAssignedRole_WithInvalidRoleId_ShouldThrowDomainException(long invalidRole)
        {
            var step = new ProcessStep(10, "Avaliação Técnica", 2, null);

            var ex = Assert.Throws<DomainException>(() => step.SetAssignedRole(invalidRole));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 6. TOUCH STABILITY / MÉTODOS MUTÁVEIS
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldNotThrow_And_ShouldAlwaysUpdateUpdatedAt()
        {
            var step = new ProcessStep(10, "Inicial", 0, null);

            // Record.Exception captura qualquer exceção sem quebrar o teste imediatamente.
            var ex1 = Record.Exception(() => step.SetName("Validação Financeira"));
            var after1 = step.UpdatedAt;

            var ex2 = Record.Exception(() => step.SetOrder(5));
            var after2 = step.UpdatedAt;

            var ex3 = Record.Exception(() => step.SetAssignedRole(42));
            var after3 = step.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);

            Assert.NotNull(after1);
            Assert.NotNull(after2);
            Assert.NotNull(after3);

            Assert.True(after2 >= after1);
            Assert.True(after3 >= after2);
        }
    }
}

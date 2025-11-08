using System;
using System.Linq;
using System.Reflection;
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
            Assert.True(t.IsSealed, "ProcessStep deve continuar sealed.");
            Assert.True(t.BaseType == typeof(BaseEntity), "ProcessStep deve herdar BaseEntity.");
        }

        [Fact]
        public void ProcessStep_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(ProcessStep);

            // ProcessId
            var processIdProp = t.GetProperty("ProcessId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(processIdProp);
            Assert.Equal(typeof(long), processIdProp!.PropertyType);
            Assert.True(processIdProp.GetGetMethod()!.IsPublic);
            Assert.Null(processIdProp.GetSetMethod());
            Assert.True(processIdProp.GetSetMethod(true)!.IsPrivate);

            // StepId (somente get)
            var stepIdProp = t.GetProperty("StepId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepIdProp);
            Assert.Equal(typeof(long), stepIdProp!.PropertyType);
            Assert.True(stepIdProp.GetGetMethod()!.IsPublic);
            Assert.Null(stepIdProp.GetSetMethod());
            Assert.Null(stepIdProp.GetSetMethod(true));

            // StepName
            var stepNameProp = t.GetProperty("StepName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepNameProp);
            Assert.Equal(typeof(string), stepNameProp!.PropertyType);
            Assert.True(stepNameProp.GetGetMethod()!.IsPublic);
            Assert.Null(stepNameProp.GetSetMethod());
            Assert.True(stepNameProp.GetSetMethod(true)!.IsPrivate);

            // StepOrder
            var stepOrderProp = t.GetProperty("StepOrder", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepOrderProp);
            Assert.Equal(typeof(int), stepOrderProp!.PropertyType);
            Assert.True(stepOrderProp.GetGetMethod()!.IsPublic);
            Assert.Null(stepOrderProp.GetSetMethod());
            Assert.True(stepOrderProp.GetSetMethod(true)!.IsPrivate);

            // AssignedRoleId
            var assignedRoleProp = t.GetProperty("AssignedRoleId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(assignedRoleProp);
            Assert.Equal(typeof(long?), assignedRoleProp!.PropertyType);
            Assert.True(assignedRoleProp.GetGetMethod()!.IsPublic);
            Assert.Null(assignedRoleProp.GetSetMethod());
            Assert.True(assignedRoleProp.GetSetMethod(true)!.IsPrivate);
        }

        [Fact]
        public void ProcessStep_Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(ProcessStep);

            // private ctor sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);
            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "Ctor sem parâmetros deve ser private (EF).");

            // ctor público esperado: (long, string, int, long?)
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

            Assert.Equal(typeof(string), t.GetMethod("SetName")!.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(int), t.GetMethod("SetOrder")!.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(long?), t.GetMethod("SetAssignedRole")!.GetParameters()[0].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR (REGRAS)
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_AllFields_AndTrimName()
        {
            var beforeUtc = DateTime.UtcNow;

            var step = new ProcessStep(
                processId: 10,
                stepName: "   Aprovar Solicitação   ",
                stepOrder: 2,
                assignedRoleId: 99
            );

            var afterUtc = DateTime.UtcNow;

            Assert.Equal(10, step.ProcessId);
            Assert.Equal("Aprovar Solicitação", step.StepName);
            Assert.Equal(2, step.StepOrder);
            Assert.Equal(99, step.AssignedRoleId);
            Assert.Equal(step.Id, step.StepId);

            // UpdatedAt deve ter sido tocado por SetName/SetOrder/SetAssignedRole
            Assert.NotNull(step.UpdatedAt);
            Assert.True(step.UpdatedAt!.Value >= beforeUtc && step.UpdatedAt!.Value <= afterUtc);

            // CreatedAt em UTC (se sua BaseEntity usa UtcNow, compare com UtcNow)
            Assert.True(step.CreatedAt >= beforeUtc && step.CreatedAt <= afterUtc,
                "CreatedAt deve estar em UTC e dentro da janela do construtor.");
        }

        [Fact]
        public void Constructor_WithNullAssignedRole_ShouldStillBeValid()
        {
            var step = new ProcessStep(22, "Assinar Contrato", 0, null);

            Assert.Equal(22, step.ProcessId);
            Assert.Equal("Assinar Contrato", step.StepName);
            Assert.Equal(0, step.StepOrder);
            Assert.Null(step.AssignedRoleId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void Constructor_InvalidProcessId_ShouldThrow(long invalidProcessId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(invalidProcessId, "Validação", 1, null));
            Assert.Equal("Invalid ProcessId.", ex.Message);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidStepName_ShouldThrow(string badName)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(10, badName, 1, null));
            Assert.Equal("Step name is required.", ex.Message);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-10)]
        public void Constructor_InvalidStepOrder_ShouldThrow(int invalidOrder)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(10, "Validação", invalidOrder, null));
            Assert.Equal("Step order must be >= 0.", ex.Message);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-99)]
        public void Constructor_InvalidAssignedRoleId_ShouldThrow(long invalidRoleId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessStep(10, "Validação", 1, invalidRoleId));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetName
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetName_WithValidValue_ShouldTrim_And_Touch()
        {
            var step = new ProcessStep(10, "Inicial", 1, null);
            var before = step.UpdatedAt ?? DateTime.MinValue;

            step.SetName("   Revisar Documentos   ");
            var after = step.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal("Revisar Documentos", step.StepName);
            Assert.True(after > before, "SetName deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetName_WithNullOrWhitespace_ShouldThrow(string badName)
        {
            var step = new ProcessStep(10, "X", 1, null);
            var ex = Assert.Throws<DomainException>(() => step.SetName(badName));
            Assert.Equal("Step name is required.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetOrder
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetOrder_WithValidValue_ShouldUpdate_And_Touch()
        {
            var step = new ProcessStep(10, "Analisar Dados", 5, null);
            var before = step.UpdatedAt ?? DateTime.MinValue;

            step.SetOrder(9);
            var after = step.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(9, step.StepOrder);
            Assert.True(after > before, "SetOrder deve chamar Touch().");
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-5)]
        public void SetOrder_WithNegativeValue_ShouldThrow(int invalidOrder)
        {
            var step = new ProcessStep(10, "Analisar Dados", 5, null);
            var ex = Assert.Throws<DomainException>(() => step.SetOrder(invalidOrder));
            Assert.Equal("Step order must be >= 0.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 5. SetAssignedRole
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetAssignedRole_WithValidRoleId_ShouldUpdate_And_Touch()
        {
            var step = new ProcessStep(10, "Avaliação Jurídica", 2, null);
            var before = step.UpdatedAt ?? DateTime.MinValue;

            step.SetAssignedRole(777);
            var after = step.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(777, step.AssignedRoleId);
            Assert.True(after > before, "SetAssignedRole deve chamar Touch().");
        }

        [Fact]
        public void SetAssignedRole_WithNull_ShouldClear_And_Touch()
        {
            var step = new ProcessStep(10, "Avaliação Técnica", 2, 123);
            var before = step.UpdatedAt ?? DateTime.MinValue;

            step.SetAssignedRole(null);
            var after = step.UpdatedAt ?? DateTime.MinValue;

            Assert.Null(step.AssignedRoleId);
            Assert.True(after > before, "SetAssignedRole(null) deve chamar Touch().");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void SetAssignedRole_WithInvalidRoleId_ShouldThrow(long invalidRole)
        {
            var step = new ProcessStep(10, "Avaliação Técnica", 2, null);
            var ex = Assert.Throws<DomainException>(() => step.SetAssignedRole(invalidRole));
            Assert.Equal("Invalid RoleId.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 6. TOUCH STABILITY / MÉTODOS MUTÁVEIS
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Mutation_Methods_ShouldAlwaysAdvance_UpdatedAt()
        {
            var step = new ProcessStep(10, "Inicial", 0, null);

            var t0 = step.UpdatedAt ?? DateTime.MinValue;

            step.SetName("Validação Financeira");
            var t1 = step.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t1 > t0);

            step.SetOrder(5);
            var t2 = step.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t2 > t1);

            step.SetAssignedRole(42);
            var t3 = step.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t3 > t2);
        }
    }
}

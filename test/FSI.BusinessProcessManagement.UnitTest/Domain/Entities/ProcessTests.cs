using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class ProcessTests
    {
        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------
        private static void SetEntityId(object entity, long id)
        {
            var prop = entity.GetType().GetProperty("Id",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(prop);

            var setter = prop!.GetSetMethod(nonPublic: true);
            Assert.NotNull(setter);

            setter!.Invoke(entity, new object?[] { id });
        }

        private static DateTime MinWhenNull(DateTime? dt) => dt ?? DateTime.MinValue;

        // ------------------------------------------------------------
        // 1) CONTRATO / ESTRUTURA
        // ------------------------------------------------------------

        [Fact]
        public void Process_Class_Must_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(Process);

            Assert.NotNull(t);
            Assert.True(t.IsSealed, "Process deve continuar sealed.");
            Assert.Equal(typeof(BaseEntity), t.BaseType);
        }

        [Fact]
        public void Process_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(Process);

            // Name
            var nameProp = t.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(nameProp);
            Assert.Equal(typeof(string), nameProp!.PropertyType);
            Assert.True(nameProp.GetGetMethod()!.IsPublic);
            Assert.Null(nameProp.GetSetMethod());
            Assert.True(nameProp.GetSetMethod(true)!.IsPrivate);

            // DepartmentId
            var deptIdProp = t.GetProperty("DepartmentId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(deptIdProp);
            Assert.Equal(typeof(long?), deptIdProp!.PropertyType);
            Assert.True(deptIdProp.GetGetMethod()!.IsPublic);
            Assert.Null(deptIdProp.GetSetMethod());
            Assert.True(deptIdProp.GetSetMethod(true)!.IsPrivate);

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType); // string? -> System.String
            Assert.True(descProp.GetGetMethod()!.IsPublic);
            Assert.Null(descProp.GetSetMethod());
            Assert.True(descProp.GetSetMethod(true)!.IsPrivate);

            // CreatedBy
            var createdByProp = t.GetProperty("CreatedBy", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(createdByProp);
            Assert.Equal(typeof(long?), createdByProp!.PropertyType);
            Assert.True(createdByProp.GetGetMethod()!.IsPublic);
            Assert.Null(createdByProp.GetSetMethod());
            Assert.True(createdByProp.GetSetMethod(true)!.IsPrivate);

            // Steps (IReadOnlyList)
            var stepsProp = t.GetProperty("Steps", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepsProp);
            Assert.True(typeof(IReadOnlyList<ProcessStep>).IsAssignableFrom(stepsProp!.PropertyType));
            Assert.True(stepsProp.GetGetMethod()!.IsPublic);
            Assert.Null(stepsProp.GetSetMethod(true)); // sem set (nem privado)
        }

        [Fact]
        public void Process_Should_Expose_ReadOnly_Steps_View_And_Cache_It()
        {
            var process = new Process("Proc F");

            Assert.NotNull(process.Steps);
            Assert.Empty(process.Steps);

            // Não deve ser List<T> exposta
            Assert.False(process.Steps is List<ProcessStep>,
                "Steps não deve ser List pública; deve ser IReadOnlyList.");

            // Como _stepsView é cacheado, múltiplas leituras devem referenciar o mesmo objeto
            var v1 = process.Steps;
            var v2 = process.Steps;
            Assert.True(object.ReferenceEquals(v1, v2), "Steps deve retornar a mesma view cacheada.");
        }

        [Fact]
        public void Process_Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(Process);

            // private ctor (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "Ctor sem parâmetros deve ser private (EF).");

            // public ctor esperado: (string name, long? departmentId = null, string? description = null, long? createdBy = null)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 4
                           && p[0].ParameterType == typeof(string)
                           && p[1].ParameterType == typeof(long?)
                           && p[2].ParameterType == typeof(string)
                           && p[3].ParameterType == typeof(long?);
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void Process_Public_Methods_Signatures_Must_Remain()
        {
            var t = typeof(Process);

            Assert.Equal(typeof(string), t.GetMethod("SetName")!.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(long?), t.GetMethod("SetDepartment")!.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(string), t.GetMethod("SetDescription")!.GetParameters()[0].ParameterType);
            Assert.Equal(typeof(long?), t.GetMethod("SetCreatedBy")!.GetParameters()[0].ParameterType);

            var addStep = t.GetMethod("AddStep");
            Assert.NotNull(addStep);
            var pAdd = addStep!.GetParameters();
            Assert.Equal(3, pAdd.Length);
            Assert.Equal(typeof(string), pAdd[0].ParameterType);
            Assert.Equal(typeof(int), pAdd[1].ParameterType);
            Assert.Equal(typeof(long?), pAdd[2].ParameterType);

            var removeStep = t.GetMethod("RemoveStep");
            Assert.NotNull(removeStep);
            Assert.Equal(typeof(long), removeStep!.GetParameters()[0].ParameterType);

            var startExecution = t.GetMethod("StartExecution");
            Assert.NotNull(startExecution);
            var pExec = startExecution!.GetParameters();
            Assert.Equal(typeof(long), pExec[0].ParameterType);
            Assert.Equal(typeof(long?), pExec[1].ParameterType);
        }

        // ------------------------------------------------------------
        // 2) CONSTRUTOR / ESTADO INICIAL
        // ------------------------------------------------------------

        [Fact]
        public void Constructor_ShouldInitialize_WithName_AndOptionalFields()
        {
            var beforeUtc = DateTime.UtcNow;

            var process = new Process(
                name: "   Processamento de Carga   ",
                departmentId: 10,
                description: "  fluxo de importação  ",
                createdBy: 777
            );

            var afterUtc = DateTime.UtcNow;

            Assert.Equal("Processamento de Carga", process.Name);
            Assert.Equal(10, process.DepartmentId);
            Assert.Equal("fluxo de importação", process.Description);
            Assert.Equal(777, process.CreatedBy);

            Assert.NotNull(process.Steps);
            Assert.Empty(process.Steps);

            // SetName() chama Touch(), então UpdatedAt deve estar na janela
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt!.Value >= beforeUtc && process.UpdatedAt!.Value <= afterUtc);
        }

        [Fact]
        public void Constructor_ShouldNullDescription_WhenBlank()
        {
            var p1 = new Process("Teste", description: null);
            Assert.Null(p1.Description);

            var p2 = new Process("Teste", description: "");
            Assert.Null(p2.Description);

            var p3 = new Process("Teste", description: "   ");
            Assert.Null(p3.Description);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_InvalidName_ShouldThrowDomainException(string invalidName)
        {
            var ex = Assert.Throws<DomainException>(() => new Process(invalidName));
            Assert.Equal("Process name is required.", ex.Message);
        }

        [Fact]
        public void Constructor_NameTooLong_ShouldThrowDomainException()
        {
            string longName = new string('X', 201);
            var ex = Assert.Throws<DomainException>(() => new Process(longName));
            Assert.Equal("Process name too long (max 200).", ex.Message);
        }

        // ------------------------------------------------------------
        // 3) SetName
        // ------------------------------------------------------------

        [Fact]
        public void SetName_WithValidName_ShouldTrim_And_UpdateUpdatedAt()
        {
            var process = new Process("Inicial");
            var before = MinWhenNull(process.UpdatedAt);

            process.SetName("   Novo Nome   ");
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Equal("Novo Nome", process.Name);
            Assert.True(after > before, "SetName deve chamar Touch() e avançar UpdatedAt.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void SetName_WithInvalidName_ShouldThrowDomainException(string invalid)
        {
            var process = new Process("Valido");
            var ex = Assert.Throws<DomainException>(() => process.SetName(invalid));
            Assert.Equal("Process name is required.", ex.Message);
        }

        [Fact]
        public void SetName_WithTooLongName_ShouldThrowDomainException()
        {
            var process = new Process("Valido");
            string longName = new string('A', 201);
            var ex = Assert.Throws<DomainException>(() => process.SetName(longName));
            Assert.Equal("Process name too long (max 200).", ex.Message);
        }

        // ------------------------------------------------------------
        // 4) SetDepartment / SetDescription / SetCreatedBy
        // ------------------------------------------------------------

        [Fact]
        public void SetDepartment_ShouldAssignValue_AndTouch()
        {
            var process = new Process("Proc A");
            var before = MinWhenNull(process.UpdatedAt);

            process.SetDepartment(99);
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Equal(99, process.DepartmentId);
            Assert.True(after > before);
        }

        [Fact]
        public void SetDepartment_ShouldAllowNull_AndTouch()
        {
            var process = new Process("Proc A", departmentId: 1);
            var before = MinWhenNull(process.UpdatedAt);

            process.SetDepartment(null);
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Null(process.DepartmentId);
            Assert.True(after > before);
        }

        [Fact]
        public void SetDescription_ShouldTrimAndSetValue_AndTouch()
        {
            var process = new Process("Proc B");
            var before = MinWhenNull(process.UpdatedAt);

            process.SetDescription("   algo importante   ");
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Equal("algo importante", process.Description);
            Assert.True(after > before);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetDescription_ShouldSetNull_WhenBlank_AndTouch(string? invalidDesc)
        {
            var process = new Process("Proc C", description: "preenchido");
            var before = MinWhenNull(process.UpdatedAt);

            process.SetDescription(invalidDesc);
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Null(process.Description);
            Assert.True(after > before);
        }

        [Fact]
        public void SetCreatedBy_ShouldAssignValue_AndTouch()
        {
            var process = new Process("Proc D");
            var before = MinWhenNull(process.UpdatedAt);

            process.SetCreatedBy(1234);
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Equal(1234, process.CreatedBy);
            Assert.True(after > before);
        }

        [Fact]
        public void SetCreatedBy_ShouldAllowNull_AndTouch()
        {
            var process = new Process("Proc E", createdBy: 42);
            var before = MinWhenNull(process.UpdatedAt);

            process.SetCreatedBy(null);
            var after = MinWhenNull(process.UpdatedAt);

            Assert.Null(process.CreatedBy);
            Assert.True(after > before);
        }

        // ------------------------------------------------------------
        // 5) Steps / AddStep / RemoveStep
        // ------------------------------------------------------------

        [Fact]
        public void Steps_MustStartEmpty_AndNotBeExternallyMutable_AndBeCachedView()
        {
            var process = new Process("Proc F");

            Assert.Empty(process.Steps);
            Assert.False(process.Steps is List<ProcessStep>);

            // cache da view
            var v1 = process.Steps;
            var v2 = process.Steps;
            Assert.True(object.ReferenceEquals(v1, v2));
        }

        [Fact]
        public void AddStep_ShouldCreateNewProcessStep_AppendToList_AndTouch()
        {
            var process = new Process("Proc G");
            SetEntityId(process, 1); // necessário para ctor de ProcessStep (processId > 0)

            var before = MinWhenNull(process.UpdatedAt);

            var step = process.AddStep(
                stepName: "  Validação Documental ",
                stepOrder: 1,
                assignedRoleId: 999
            );

            var after = MinWhenNull(process.UpdatedAt);

            Assert.NotNull(step);
            Assert.Equal("Validação Documental", step.StepName);
            Assert.Equal(1, step.StepOrder);
            Assert.Equal(999, step.AssignedRoleId);

            Assert.Single(process.Steps);
            Assert.Contains(step, process.Steps);

            Assert.True(after > before);
        }

        [Fact]
        public void AddStep_WithDuplicateOrder_ShouldThrowDomainException_AndNotAdd()
        {
            var process = new Process("Proc H");
            SetEntityId(process, 1);

            process.AddStep("Primeira", 1);

            var ex = Assert.Throws<DomainException>(() => process.AddStep("Outra mesma ordem", 1));
            Assert.Equal("A step with order 1 already exists.", ex.Message);

            Assert.Single(process.Steps);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void AddStep_WithInvalidName_ShouldThrowDomainException(string invalidStepName)
        {
            var process = new Process("Proc I");
            SetEntityId(process, 1);

            var ex = Assert.Throws<DomainException>(() => process.AddStep(invalidStepName, stepOrder: 1));
            Assert.Equal("StepName is required.", ex.Message);
        }

        [Fact]
        public void RemoveStep_ShouldRemoveById_AndTouch()
        {
            var process = new Process("Proc J");
            SetEntityId(process, 1);

            var s1 = process.AddStep("A", 1);
            var s2 = process.AddStep("B", 2);

            // Definimos o Id do s2 para permitir remoção por Id
            SetEntityId(s2, 22);

            var before = MinWhenNull(process.UpdatedAt);

            process.RemoveStep(22);

            var after = MinWhenNull(process.UpdatedAt);

            Assert.Single(process.Steps);
            Assert.Contains(s1, process.Steps);
            Assert.DoesNotContain(s2, process.Steps);
            Assert.True(after > before);
        }

        [Fact]
        public void RemoveStep_InvalidId_ShouldThrowDomainException()
        {
            var process = new Process("Proc K");
            SetEntityId(process, 1);

            var s1 = process.AddStep("A", 1);
            SetEntityId(s1, 11);

            var ex = Assert.Throws<DomainException>(() => process.RemoveStep(stepId: 999999));
            Assert.Equal("Step not found.", ex.Message);

            Assert.Single(process.Steps);
        }

        // ------------------------------------------------------------
        // 6) StartExecution
        // ------------------------------------------------------------

        [Fact]
        public void StartExecution_WithValidStepId_ShouldReturnProcessExecution_AndTouch()
        {
            var process = new Process("Proc L");
            SetEntityId(process, 1);

            var step = process.AddStep("Check docs", 1);
            SetEntityId(step, 101); // garantir correspondência para validação de pertença

            var before = MinWhenNull(process.UpdatedAt);

            var exec = process.StartExecution(stepId: 101, userId: 444);

            var after = MinWhenNull(process.UpdatedAt);

            Assert.NotNull(exec);
            Assert.IsType<ProcessExecution>(exec);

            Assert.Equal(1, exec.ProcessId);
            Assert.Equal(101, exec.StepId);
            Assert.Equal(444, exec.UserId);

            Assert.True(after > before);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void StartExecution_WithInvalidStepId_ShouldThrowDomainException(long invalidStep)
        {
            var process = new Process("Proc M");
            SetEntityId(process, 1);

            var ex = Assert.Throws<DomainException>(() => process.StartExecution(invalidStep, userId: 123));
            Assert.Equal("StepId is invalid.", ex.Message);
        }

        [Fact]
        public void StartExecution_WhenStepDoesNotBelongToProcess_ShouldThrowDomainException()
        {
            var process = new Process("Proc N");
            SetEntityId(process, 1);

            // StepId inexistente na coleção do processo
            var ex = Assert.Throws<DomainException>(() => process.StartExecution(stepId: 777, userId: 99));
            Assert.Equal("Step does not belong to this process.", ex.Message);
        }
    }
}

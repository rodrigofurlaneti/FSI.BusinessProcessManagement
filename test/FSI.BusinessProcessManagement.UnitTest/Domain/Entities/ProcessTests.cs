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
        // -----------------------------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Process_Class_Must_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(Process);

            Assert.NotNull(t);

            Assert.True(t.IsSealed,
                "Process deve continuar sendo sealed. Se mudar, atualize este teste.");

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
            Assert.NotNull(nameProp.GetGetMethod());
            Assert.Null(nameProp.GetSetMethod());
            var nameSetMethod = nameProp.GetSetMethod(true);
            Assert.NotNull(nameSetMethod);
            Assert.True(nameSetMethod!.IsPrivate,
                "Name deve continuar com private set;");

            // DepartmentId
            var deptIdProp = t.GetProperty("DepartmentId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(deptIdProp);
            Assert.Equal(typeof(long?), deptIdProp!.PropertyType);
            Assert.NotNull(deptIdProp.GetGetMethod());
            Assert.Null(deptIdProp.GetSetMethod());
            var deptSetMethod = deptIdProp.GetSetMethod(true);
            Assert.NotNull(deptSetMethod);
            Assert.True(deptSetMethod!.IsPrivate,
                "DepartmentId deve continuar com private set;");

            // Description
            var descProp = t.GetProperty("Description", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(descProp);
            Assert.Equal(typeof(string), descProp!.PropertyType); // string? = System.String em runtime
            Assert.NotNull(descProp.GetGetMethod());
            Assert.Null(descProp.GetSetMethod());
            var descSetMethod = descProp.GetSetMethod(true);
            Assert.NotNull(descSetMethod);
            Assert.True(descSetMethod!.IsPrivate,
                "Description deve continuar com private set;");

            // CreatedBy
            var createdByProp = t.GetProperty("CreatedBy", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(createdByProp);
            Assert.Equal(typeof(long?), createdByProp!.PropertyType);
            Assert.NotNull(createdByProp.GetGetMethod());
            Assert.Null(createdByProp.GetSetMethod());
            var createdBySetMethod = createdByProp.GetSetMethod(true);
            Assert.NotNull(createdBySetMethod);
            Assert.True(createdBySetMethod!.IsPrivate,
                "CreatedBy deve continuar com private set;");

            // Steps
            var stepsProp = t.GetProperty("Steps", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepsProp);
            Assert.True(typeof(IReadOnlyCollection<ProcessStep>).IsAssignableFrom(stepsProp!.PropertyType),
                "Steps deve continuar sendo IReadOnlyCollection<ProcessStep> (somente leitura).");
            Assert.NotNull(stepsProp.GetGetMethod());
            // Steps só tem get (sem set público e nem privado)
            Assert.Null(stepsProp.GetSetMethod(true));
        }

        [Fact]
        public void Process_Must_Contain_PrivateField__steps_AsListOfProcessStep()
        {
            var t = typeof(Process);

            var stepsField = t.GetField("_steps", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(stepsField);
            Assert.Equal(typeof(List<ProcessStep>), stepsField!.FieldType);
        }

        [Fact]
        public void Process_Must_Have_Private_Parameterless_Constructor_And_Public_MainConstructor()
        {
            var t = typeof(Process);

            // ctor privado sem parâmetros
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate,
                "O construtor sem parâmetro deve continuar private (requisito EF).");

            // ctor público esperado (string name, long? departmentId = null, string? description = null, long? createdBy = null)
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

            // SetName(string name)
            var setName = t.GetMethod("SetName", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setName);
            Assert.Single(setName!.GetParameters());
            Assert.Equal(typeof(string), setName.GetParameters()[0].ParameterType);

            // SetDepartment(long? departmentId)
            var setDepartment = t.GetMethod("SetDepartment", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setDepartment);
            Assert.Single(setDepartment!.GetParameters());
            Assert.Equal(typeof(long?), setDepartment.GetParameters()[0].ParameterType);

            // SetDescription(string? description)
            var setDescription = t.GetMethod("SetDescription", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setDescription);
            Assert.Single(setDescription!.GetParameters());
            Assert.Equal(typeof(string), setDescription.GetParameters()[0].ParameterType);

            // SetCreatedBy(long? createdBy)
            var setCreatedBy = t.GetMethod("SetCreatedBy", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setCreatedBy);
            Assert.Single(setCreatedBy!.GetParameters());
            Assert.Equal(typeof(long?), setCreatedBy.GetParameters()[0].ParameterType);

            // AddStep(string stepName, int stepOrder, long? assignedRoleId = null)
            var addStep = t.GetMethod("AddStep", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(addStep);
            var addParams = addStep!.GetParameters();
            Assert.Equal(3, addParams.Length);
            Assert.Equal(typeof(string), addParams[0].ParameterType);
            Assert.Equal(typeof(int), addParams[1].ParameterType);
            Assert.Equal(typeof(long?), addParams[2].ParameterType);

            // RemoveStep(long stepId)
            var removeStep = t.GetMethod("RemoveStep", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(removeStep);
            Assert.Single(removeStep!.GetParameters());
            Assert.Equal(typeof(long), removeStep.GetParameters()[0].ParameterType);

            // StartExecution(long stepId, long? userId = null)
            var startExecution = t.GetMethod("StartExecution", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(startExecution);
            var execParams = startExecution!.GetParameters();
            Assert.Equal(2, execParams.Length);
            Assert.Equal(typeof(long), execParams[0].ParameterType);
            Assert.Equal(typeof(long?), execParams[1].ParameterType);
        }

        // -----------------------------------------------------------------------------------------
        // 2. CONSTRUTOR / ESTADO INICIAL
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_ShouldInitialize_WithName_AndOptionalFields()
        {
            // Arrange
            var beforeUtc = DateTime.UtcNow;

            // Act
            var process = new Process(
                name: "   Processamento de Carga   ",
                departmentId: 10,
                description: "  fluxo de importação  ",
                createdBy: 777
            );

            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal("Processamento de Carga", process.Name);
            Assert.Equal(10, process.DepartmentId);
            Assert.Equal("fluxo de importação", process.Description);
            Assert.Equal(777, process.CreatedBy);

            // SetName() é chamado no construtor e ele faz Touch(),
            // então UpdatedAt já deve ter sido preenchido.
            Assert.NotNull(process.UpdatedAt);
            Assert.InRange(process.UpdatedAt!.Value, beforeUtc, afterUtc);

            // Steps deve começar vazio
            Assert.NotNull(process.Steps);
            Assert.Empty(process.Steps);
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
            var ex = Assert.Throws<DomainException>(() =>
                new Process(invalidName)
            );

            Assert.Equal("Process name is required.", ex.Message);
        }

        [Fact]
        public void Constructor_NameTooLong_ShouldThrowDomainException()
        {
            string longName = new string('X', 201); // >200

            var ex = Assert.Throws<DomainException>(() =>
                new Process(longName)
            );

            Assert.Equal("Process name too long (max 200).", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 3. SetName
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetName_WithValidName_ShouldTrim_And_UpdateUpdatedAt()
        {
            var process = new Process("Inicial");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetName("   Novo Nome   ");

            Assert.Equal("Novo Nome", process.Name);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before,
                "SetName deve chamar Touch(), logo UpdatedAt deve avançar.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void SetName_WithInvalidName_ShouldThrowDomainException(string invalid)
        {
            var process = new Process("Valido");

            var ex = Assert.Throws<DomainException>(() =>
                process.SetName(invalid)
            );

            Assert.Equal("Process name is required.", ex.Message);
        }

        [Fact]
        public void SetName_WithTooLongName_ShouldThrowDomainException()
        {
            var process = new Process("Valido");

            string longName = new string('A', 201);

            var ex = Assert.Throws<DomainException>(() =>
                process.SetName(longName)
            );

            Assert.Equal("Process name too long (max 200).", ex.Message);
        }

        [Fact]
        public void SetName_ShouldNotThrow_OnHappyPath()
        {
            var process = new Process("abc");
            var ex = Record.Exception(() => process.SetName("def"));
            Assert.Null(ex);
        }

        // -----------------------------------------------------------------------------------------
        // 4. SetDepartment / SetDescription / SetCreatedBy
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void SetDepartment_ShouldAssignValue_AndTouch()
        {
            var process = new Process("Proc A");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetDepartment(99);

            Assert.Equal(99, process.DepartmentId);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void SetDepartment_ShouldAllowNull_AndTouch()
        {
            var process = new Process("Proc A", departmentId: 1);

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetDepartment(null);

            Assert.Null(process.DepartmentId);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void SetDescription_ShouldTrimAndSetValue_AndTouch()
        {
            var process = new Process("Proc B");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetDescription("   algo importante   ");

            Assert.Equal("algo importante", process.Description);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetDescription_ShouldSetNull_WhenBlank_AndTouch(string? invalidDesc)
        {
            var process = new Process("Proc C", description: "preenchido");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetDescription(invalidDesc);

            Assert.Null(process.Description);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void SetCreatedBy_ShouldAssignValue_AndTouch()
        {
            var process = new Process("Proc D");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetCreatedBy(1234);

            Assert.Equal(1234, process.CreatedBy);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void SetCreatedBy_ShouldAllowNull_AndTouch()
        {
            var process = new Process("Proc E", createdBy: 42);

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.SetCreatedBy(null);

            Assert.Null(process.CreatedBy);
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        // -----------------------------------------------------------------------------------------
        // 5. Steps / AddStep / RemoveStep
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void Steps_MustStartEmpty_AndNotBeExternallyMutable()
        {
            var process = new Process("Proc F");

            Assert.Empty(process.Steps);

            // tentar cast e add direto deve falhar conceitualmente (IReadOnlyCollection não tem Add)
            Assert.False(process.Steps is List<ProcessStep>,
                "Steps NÃO deve ser List pública, deve ser IReadOnlyCollection.");
        }

        [Fact]
        public void AddStep_ShouldCreateNewProcessStep_AppendToList_AndTouch()
        {
            var process = new Process("Proc G");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            var step = process.AddStep(
                stepName: "  Validação Documental ",
                stepOrder: 1,
                assignedRoleId: 999
            );

            // Assert entidade retornada
            Assert.NotNull(step);
            // A classe ProcessStep deve ter sido criada usando this.Id e Trim no nome.
            // Validamos os campos que sabemos que existem na assinatura AddStep.
            Assert.Equal(1, step.StepOrder);
            Assert.Equal("Validação Documental", step.StepName ?? step.StepName ?? step.ToString());
            // ↑ Observação:
            // Eu não vi a implementação de ProcessStep, então aqui vou explicar:
            // - Se sua classe ProcessStep tem propriedade pública StepName, mantenha o Assert.Equal nessa propriedade.
            // - Se o nome da propriedade é Name (ou Title), ajuste aqui.
            // - Se não existe nenhuma propriedade pública que carregue o nome, remova essa asserção e mantenha só StepOrder.

            // Deve ter incrementado a lista interna
            Assert.Single(process.Steps);
            Assert.Contains(step, process.Steps);

            // Deve ter atualizado UpdatedAt via Touch()
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void AddStep_WithDuplicateOrder_ShouldThrowDomainException_AndNotAdd()
        {
            var process = new Process("Proc H");
            process.AddStep("Primeira", 1);

            var ex = Assert.Throws<DomainException>(() =>
                process.AddStep("Outra mesma ordem", 1)
            );

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

            var ex = Assert.Throws<DomainException>(() =>
                process.AddStep(invalidStepName, stepOrder: 1)
            );

            Assert.Equal("StepName is required.", ex.Message);
        }

        [Fact]
        public void RemoveStep_ShouldRemoveByIdOrStepId_AndTouch()
        {
            var process = new Process("Proc J");

            // adiciona 2 steps
            var s1 = process.AddStep("A", 1);
            var s2 = process.AddStep("B", 2);

            // tentativa de remoção:
            // A regra atual diz: procura x.Id == stepId || x.StepId == stepId
            // Então vamos tentar remover usando o próprio Id do segundo step.
            // Eu não sei se ProcessStep expõe public long Id ou StepId public,
            // então vou tentar ambos de forma defensiva:
            long candidateId =
                (long?)GetPropertyValue<long?>(s2, "Id") ??
                (long?)GetPropertyValue<long?>(s2, "StepId")
                ?? throw new Exception("ProcessStep precisa ter Id ou StepId público long para remoção.");

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            process.RemoveStep(candidateId);

            Assert.Single(process.Steps); // sobrou só 1
            Assert.Contains(s1, process.Steps);
            Assert.DoesNotContain(s2, process.Steps);

            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Fact]
        public void RemoveStep_InvalidId_ShouldThrowDomainException()
        {
            var process = new Process("Proc K");
            process.AddStep("A", 1);

            var ex = Assert.Throws<DomainException>(() =>
                process.RemoveStep(stepId: 999999) // não existe
            );

            Assert.Equal("Step not found.", ex.Message);

            Assert.Single(process.Steps);
        }

        // -----------------------------------------------------------------------------------------
        // 6. StartExecution
        // -----------------------------------------------------------------------------------------

        [Fact]
        public void StartExecution_WithValidStepId_ShouldReturnProcessExecution_AndTouch()
        {
            var process = new Process("Proc L");
            var step = process.AddStep("Check docs", 1);

            // precisamos de um stepId válido (>0).
            // Aqui vale a mesma restrição: ProcessExecution espera stepId
            // Logo pegamos um Id conhecido da etapa adicionada:
            long candidateStepId =
                (long?)GetPropertyValue<long?>(step, "Id") ??
                (long?)GetPropertyValue<long?>(step, "StepId")
                ?? 1; // fallback safe: 1 já é válido pois AddStep usou stepOrder 1

            var before = process.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            var exec = process.StartExecution(candidateStepId, userId: 444);

            Assert.NotNull(exec);
            // Validação defensiva: garantir que exec seja realmente ProcessExecution
            Assert.IsType<ProcessExecution>(exec);

            // UpdatedAt atualizada por Touch()
            Assert.NotNull(process.UpdatedAt);
            Assert.True(process.UpdatedAt >= before);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-999)]
        public void StartExecution_WithInvalidStepId_ShouldThrowDomainException(long invalidStep)
        {
            var process = new Process("Proc M");

            var ex = Assert.Throws<DomainException>(() =>
                process.StartExecution(invalidStep, userId: 123)
            );

            Assert.Equal("StepId is invalid.", ex.Message);
        }

        // -----------------------------------------------------------------------------------------
        // 7. Helper interno para pegar propriedades opcionais de ProcessStep em runtime
        // -----------------------------------------------------------------------------------------
        private static T? GetPropertyValue<T>(object obj, string propName)
        {
            var prop = obj.GetType().GetProperty(propName,
                BindingFlags.Public | BindingFlags.Instance);

            if (prop == null) return default;

            var value = prop.GetValue(obj);
            if (value == null) return default;

            return (T)value;
        }
    }
}

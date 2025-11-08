using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Enums;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Entities
{
    public class ProcessExecutionTests
    {
        // ------------------------------------------------------------------------------------------------
        // 1. CONTRATO ESTRUTURAL
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void ProcessExecution_Class_Must_Exist_And_Be_Sealed_And_Inherit_BaseEntity()
        {
            var t = typeof(ProcessExecution);

            Assert.NotNull(t);
            Assert.True(t.IsSealed, "ProcessExecution deve continuar sealed. Se mudar, atualize o teste.");
            Assert.True(t.BaseType == typeof(BaseEntity),
                "ProcessExecution deve continuar herdando BaseEntity. Se mudar a herança, atualize o teste.");
        }

        [Fact]
        public void ProcessExecution_Properties_Must_Exist_With_Expected_Types_And_PrivateSetters()
        {
            var t = typeof(ProcessExecution);

            // ProcessId
            var processIdProp = t.GetProperty("ProcessId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(processIdProp);
            Assert.Equal(typeof(long), processIdProp!.PropertyType);
            Assert.NotNull(processIdProp.GetGetMethod());
            Assert.True(processIdProp.GetSetMethod(true)!.IsPrivate, "ProcessId deve continuar com private set;");

            // StepId
            var stepIdProp = t.GetProperty("StepId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepIdProp);
            Assert.Equal(typeof(long), stepIdProp!.PropertyType);
            Assert.NotNull(stepIdProp.GetGetMethod());
            Assert.True(stepIdProp.GetSetMethod(true)!.IsPrivate, "StepId deve continuar com private set;");

            // UserId
            var userIdProp = t.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userIdProp);
            Assert.Equal(typeof(long?), userIdProp!.PropertyType);
            Assert.NotNull(userIdProp.GetGetMethod());
            Assert.True(userIdProp.GetSetMethod(true)!.IsPrivate, "UserId deve continuar com private set;");

            // Status
            var statusProp = t.GetProperty("Status", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(statusProp);
            Assert.Equal(typeof(ExecutionStatus), statusProp!.PropertyType);
            Assert.NotNull(statusProp.GetGetMethod());
            Assert.True(statusProp.GetSetMethod(true)!.IsPrivate, "Status deve continuar com private set;");

            // StartedAt
            var startedAtProp = t.GetProperty("StartedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(startedAtProp);
            Assert.Equal(typeof(DateTime?), startedAtProp!.PropertyType);
            Assert.NotNull(startedAtProp.GetGetMethod());
            Assert.True(startedAtProp.GetSetMethod(true)!.IsPrivate, "StartedAt deve continuar com private set;");

            // CompletedAt
            var completedAtProp = t.GetProperty("CompletedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(completedAtProp);
            Assert.Equal(typeof(DateTime?), completedAtProp!.PropertyType);
            Assert.NotNull(completedAtProp.GetGetMethod());
            Assert.True(completedAtProp.GetSetMethod(true)!.IsPrivate, "CompletedAt deve continuar com private set;");

            // Remarks
            var remarksProp = t.GetProperty("Remarks", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(remarksProp);
            Assert.Equal(typeof(string), remarksProp!.PropertyType); // string? -> System.String
            Assert.NotNull(remarksProp.GetGetMethod());
            Assert.True(remarksProp.GetSetMethod(true)!.IsPrivate, "Remarks deve continuar com private set;");
        }

        [Fact]
        public void ProcessExecution_Must_Have_Private_Parameterless_Constructor_And_Public_Constructor_Signature()
        {
            var t = typeof(ProcessExecution);

            // ctor privado sem parâmetros (EF)
            var privateCtor = t
                .GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(c => c.GetParameters().Length == 0);

            Assert.NotNull(privateCtor);
            Assert.True(privateCtor!.IsPrivate, "O construtor vazio deve continuar private. Se mudar, atualize o teste.");

            // ctor público (long processId, long stepId, long? userId = null)
            var publicCtor = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(c =>
                {
                    var p = c.GetParameters();
                    return p.Length == 3
                        && p[0].ParameterType == typeof(long)
                        && p[1].ParameterType == typeof(long)
                        && p[2].ParameterType == typeof(long?);
                });

            Assert.NotNull(publicCtor);
        }

        [Fact]
        public void ProcessExecution_Public_Methods_Must_Exist_With_Expected_Signatures()
        {
            var t = typeof(ProcessExecution);

            // SetStep(long stepId)
            var setStep = t.GetMethod("SetStep", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setStep);
            Assert.Single(setStep!.GetParameters());
            Assert.Equal(typeof(long), setStep.GetParameters()[0].ParameterType);

            // SetUser(long? userId)
            var setUser = t.GetMethod("SetUser", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setUser);
            Assert.Single(setUser!.GetParameters());
            Assert.Equal(typeof(long?), setUser.GetParameters()[0].ParameterType);

            // SetStatus(ExecutionStatus status)
            var setStatus = t.GetMethod("SetStatus", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setStatus);
            Assert.Single(setStatus!.GetParameters());
            Assert.Equal(typeof(ExecutionStatus), setStatus.GetParameters()[0].ParameterType);

            // SetRemarks(string? remarks)
            var setRemarks = t.GetMethod("SetRemarks", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setRemarks);
            Assert.Single(setRemarks!.GetParameters());
            Assert.Equal(typeof(string), setRemarks.GetParameters()[0].ParameterType);

            // SetTimes(DateTime? startedAtUtc, DateTime? completedAtUtc)
            var setTimes = t.GetMethod("SetTimes", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(setTimes);
            var pTimes = setTimes!.GetParameters();
            Assert.Equal(2, pTimes.Length);
            Assert.Equal(typeof(DateTime?), pTimes[0].ParameterType);
            Assert.Equal(typeof(DateTime?), pTimes[1].ParameterType);

            // Start(long? userId = null)
            var start = t.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(start);
            Assert.Single(start!.GetParameters());
            Assert.Equal(typeof(long?), start.GetParameters()[0].ParameterType);

            // Complete(string? remarks = null)
            var complete = t.GetMethod("Complete", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(complete);
            Assert.Single(complete!.GetParameters());
            Assert.Equal(typeof(string), complete.GetParameters()[0].ParameterType);

            // Cancel(string? remarks = null)
            var cancel = t.GetMethod("Cancel", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(cancel);
            Assert.Single(cancel!.GetParameters());
            Assert.Equal(typeof(string), cancel.GetParameters()[0].ParameterType);
        }

        // ------------------------------------------------------------------------------------------------
        // 2. CONSTRUTOR (REGRAS INICIAIS)
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Constructor_WithValidArguments_ShouldInitialize_State_AsInitiated()
        {
            // Arrange
            var beforeUtc = DateTime.UtcNow;

            // Act
            var exec = new ProcessExecution(processId: 10, stepId: 20, userId: 30);
            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal(10, exec.ProcessId);
            Assert.Equal(20, exec.StepId);
            Assert.Equal(30, exec.UserId);

            // Regra atual: ao criar já inicia em "Iniciado"
            Assert.Equal(ExecutionStatus.Iniciado, exec.Status);

            Assert.NotNull(exec.StartedAt);
            Assert.True(exec.StartedAt >= beforeUtc && exec.StartedAt <= afterUtc,
                "StartedAt deve ser definido no construtor com UtcNow.");

            Assert.Null(exec.CompletedAt);
            Assert.Null(exec.Remarks);
            // UpdatedAt pode ser null no construtor (Touch é chamado nos mutadores)
        }

        [Fact]
        public void Constructor_WithNullUser_ShouldStillBeValid()
        {
            var exec = new ProcessExecution(processId: 99, stepId: 7, userId: null);

            Assert.Equal(99, exec.ProcessId);
            Assert.Equal(7, exec.StepId);
            Assert.Null(exec.UserId);
            Assert.Equal(ExecutionStatus.Iniciado, exec.Status);
            Assert.NotNull(exec.StartedAt);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(-1, 1)]
        [InlineData(-100, 1)]
        public void Constructor_InvalidProcessId_ShouldThrowDomainException(long processId, long stepId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessExecution(processId, stepId, userId: null)
            );
            Assert.Equal("ProcessId is invalid.", ex.Message);
        }

        [Theory]
        [InlineData(1, 0)]
        [InlineData(1, -1)]
        [InlineData(1, -10)]
        public void Constructor_InvalidStepId_ShouldThrowDomainException(long processId, long stepId)
        {
            var ex = Assert.Throws<DomainException>(() =>
                new ProcessExecution(processId, stepId, userId: null)
            );
            Assert.Equal("StepId is invalid.", ex.Message);
        }

        // ------------------------------------------------------------------------------------------------
        // 3. SetStep
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetStep_WithValidId_ShouldUpdateStepId_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetStep(99);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(99, exec.StepId);
            Assert.True(after > before,
                $"Expected UpdatedAt to be greater than before. Before={before:o}, After={after:o}");
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-50)]
        public void SetStep_WithInvalidId_ShouldThrowDomainException(long invalid)
        {
            var exec = new ProcessExecution(10, 20, 30);
            var ex = Assert.Throws<DomainException>(() => exec.SetStep(invalid));
            Assert.Equal("StepId is invalid.", ex.Message);
        }

        // ------------------------------------------------------------------------------------------------
        // 4. SetUser
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetUser_ShouldUpdateUserId_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, null);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetUser(123);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(123, exec.UserId);
            Assert.True(after > before,
                $"Expected UpdatedAt to be greater than before. Before={before:o}, After={after:o}");
        }

        [Fact]
        public void SetUser_ShouldAcceptNull_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 99);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetUser(null);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Null(exec.UserId);
            Assert.True(after > before,
                $"Expected UpdatedAt to be greater than before. Before={before:o}, After={after:o}");
        }

        // ------------------------------------------------------------------------------------------------
        // 5. SetStatus
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetStatus_ShouldUpdateStatus_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetStatus(ExecutionStatus.Cancelado);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.True(after > before, "SetStatus deve chamar Touch().");
        }

        // ------------------------------------------------------------------------------------------------
        // 6. SetRemarks
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetRemarks_WithValidText_ShouldTrimAndUpdate_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetRemarks("   algum apontamento importante   ");
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal("algum apontamento importante", exec.Remarks);
            Assert.True(after > before, "SetRemarks deve chamar Touch().");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetRemarks_WithNullOrWhitespace_ShouldBecomeNull_AndTouch(string? input)
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetRemarks(input);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Null(exec.Remarks);
            Assert.True(after > before, "SetRemarks deve chamar Touch() mesmo limpando.");
        }

        // ------------------------------------------------------------------------------------------------
        // 7. SetTimes
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetTimes_ShouldAssignStartedAndCompletedAndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);

            var newStart = DateTime.UtcNow.AddHours(-1);
            var newEnd = DateTime.UtcNow;

            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetTimes(newStart, newEnd);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(newStart, exec.StartedAt);
            Assert.Equal(newEnd, exec.CompletedAt);
            Assert.True(after > before, "SetTimes deve chamar Touch().");
        }

        [Fact]
        public void SetTimes_ShouldAcceptNulls_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetTimes(null, null);
            var after = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Null(exec.StartedAt);
            Assert.Null(exec.CompletedAt);
            Assert.True(after > before);
        }

        // ------------------------------------------------------------------------------------------------
        // 8. Start
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Start_ShouldSetStatusToIniciado_SetStartedAtUtcNow_ResetCompletedAt_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetStatus(ExecutionStatus.Pendente); // força outro estado
            exec.SetTimes(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1)); // simula já ter completado
            exec.SetRemarks("antigo");

            var beforeUtc = DateTime.UtcNow;
            var beforeUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;

            exec.Start(userId: 777);
            var afterUtc = DateTime.UtcNow;
            var afterUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(777, exec.UserId);
            Assert.Equal(ExecutionStatus.Iniciado, exec.Status);

            Assert.NotNull(exec.StartedAt);
            Assert.True(exec.StartedAt >= beforeUtc && exec.StartedAt <= afterUtc,
                "Start() deve atualizar StartedAt com UtcNow.");

            Assert.Null(exec.CompletedAt);
            Assert.True(afterUpdatedAt > beforeUpdatedAt, "Start() deve chamar Touch().");
        }

        [Fact]
        public void Start_ShouldNotOverrideUserId_WhenUserIdParameterIsNull()
        {
            var exec = new ProcessExecution(10, 20, 30); // já tem UserId=30
            var beforeUser = exec.UserId;

            exec.Start(userId: null);

            Assert.Equal(beforeUser, exec.UserId);
            Assert.Equal(ExecutionStatus.Iniciado, exec.Status);
            Assert.NotNull(exec.StartedAt);
            Assert.Null(exec.CompletedAt);
        }

        // ------------------------------------------------------------------------------------------------
        // 9. Complete
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Complete_ShouldSetStatusToConcluido_SetCompletedAtUtcNow_KeepOrUpdateRemarks_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks(" já tinha observação ");
            var beforeUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;
            var beforeUtc = DateTime.UtcNow;

            exec.Complete("   finalizado sem pendências   ");
            var afterUtc = DateTime.UtcNow;
            var afterUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(ExecutionStatus.Concluido, exec.Status);

            Assert.NotNull(exec.CompletedAt);
            Assert.True(exec.CompletedAt >= beforeUtc && exec.CompletedAt <= afterUtc,
                "Complete() deve definir CompletedAt = UtcNow.");

            Assert.Equal("finalizado sem pendências", exec.Remarks);
            Assert.True(afterUpdatedAt > beforeUpdatedAt, "Complete() deve chamar Touch().");
        }

        [Fact]
        public void Complete_WithNullRemarks_ShouldKeepExistingRemarks()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            exec.Complete(null);

            Assert.Equal(ExecutionStatus.Concluido, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);
        }

        [Fact]
        public void Complete_WithWhitespaceRemarks_ShouldKeepExistingRemarks()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            exec.Complete("     ");

            Assert.Equal(ExecutionStatus.Concluido, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);
        }

        // ------------------------------------------------------------------------------------------------
        // 10. Cancel
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Cancel_ShouldSetStatusToCancelado_SetCompletedAtUtcNow_KeepOrUpdateRemarks_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("chamado reaberto por falha");
            var beforeUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;
            var beforeUtc = DateTime.UtcNow;

            exec.Cancel("   cancelamento solicitado pelo cliente   ");
            var afterUtc = DateTime.UtcNow;
            var afterUpdatedAt = exec.UpdatedAt ?? DateTime.MinValue;

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);

            Assert.NotNull(exec.CompletedAt);
            Assert.True(exec.CompletedAt >= beforeUtc && exec.CompletedAt <= afterUtc,
                "Cancel() deve definir CompletedAt = UtcNow.");

            Assert.Equal("cancelamento solicitado pelo cliente", exec.Remarks);
            Assert.True(afterUpdatedAt > beforeUpdatedAt, "Cancel() deve chamar Touch().");
        }

        [Fact]
        public void Cancel_WithNullRemarks_ShouldKeepExistingRemarks()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            exec.Cancel(null);

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);
        }

        [Fact]
        public void Cancel_WithWhitespaceRemarks_ShouldKeepExistingRemarks()
        {
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            exec.Cancel("     ");

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);
        }

        // ------------------------------------------------------------------------------------------------
        // 11. MUTATION STABILITY / TOUCH()
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void AllMutationMethods_ShouldUpdateUpdatedAt_InOrder()
        {
            var exec = new ProcessExecution(10, 20, 30);

            // Garantimos progressão estrita (>) em cada chamada.
            var t0 = exec.UpdatedAt ?? DateTime.MinValue;

            exec.SetStep(77);
            var t1 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t1 > t0, "SetStep deve chamar Touch().");

            exec.SetUser(88);
            var t2 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t2 > t1, "SetUser deve chamar Touch().");

            exec.SetStatus(ExecutionStatus.Pendente);
            var t3 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t3 > t2, "SetStatus deve chamar Touch().");

            exec.SetRemarks("teste touch");
            var t4 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t4 > t3, "SetRemarks deve chamar Touch().");

            exec.SetTimes(DateTime.UtcNow, DateTime.UtcNow);
            var t5 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t5 > t4, "SetTimes deve chamar Touch().");

            exec.Start(userId: 123);
            var t6 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t6 > t5, "Start deve chamar Touch().");

            exec.Complete("done");
            var t7 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t7 > t6, "Complete deve chamar Touch().");

            exec.Cancel("cancel");
            var t8 = exec.UpdatedAt ?? DateTime.MinValue;
            Assert.True(t8 > t7, "Cancel deve chamar Touch().");
        }
    }
}

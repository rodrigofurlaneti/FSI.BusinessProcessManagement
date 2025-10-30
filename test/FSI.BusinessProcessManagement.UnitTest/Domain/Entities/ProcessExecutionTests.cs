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

            Assert.True(t.IsSealed,
                "ProcessExecution deve continuar sealed. Se mudar, atualize o teste.");

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
            Assert.Null(processIdProp.GetSetMethod());
            Assert.True(processIdProp.GetSetMethod(true)!.IsPrivate,
                "ProcessId deve continuar com private set;");

            // StepId
            var stepIdProp = t.GetProperty("StepId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(stepIdProp);
            Assert.Equal(typeof(long), stepIdProp!.PropertyType);
            Assert.NotNull(stepIdProp.GetGetMethod());
            Assert.Null(stepIdProp.GetSetMethod());
            Assert.True(stepIdProp.GetSetMethod(true)!.IsPrivate,
                "StepId deve continuar com private set;");

            // UserId
            var userIdProp = t.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(userIdProp);
            Assert.Equal(typeof(long?), userIdProp!.PropertyType);
            Assert.NotNull(userIdProp.GetGetMethod());
            Assert.Null(userIdProp.GetSetMethod());
            Assert.True(userIdProp.GetSetMethod(true)!.IsPrivate,
                "UserId deve continuar com private set;");

            // Status
            var statusProp = t.GetProperty("Status", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(statusProp);
            Assert.Equal(typeof(ExecutionStatus), statusProp!.PropertyType);
            Assert.NotNull(statusProp.GetGetMethod());
            Assert.Null(statusProp.GetSetMethod());
            Assert.True(statusProp.GetSetMethod(true)!.IsPrivate,
                "Status deve continuar com private set;");

            // StartedAt
            var startedAtProp = t.GetProperty("StartedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(startedAtProp);
            Assert.Equal(typeof(DateTime?), startedAtProp!.PropertyType);
            Assert.NotNull(startedAtProp.GetGetMethod());
            Assert.Null(startedAtProp.GetSetMethod());
            Assert.True(startedAtProp.GetSetMethod(true)!.IsPrivate,
                "StartedAt deve continuar com private set;");

            // CompletedAt
            var completedAtProp = t.GetProperty("CompletedAt", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(completedAtProp);
            Assert.Equal(typeof(DateTime?), completedAtProp!.PropertyType);
            Assert.NotNull(completedAtProp.GetGetMethod());
            Assert.Null(completedAtProp.GetSetMethod());
            Assert.True(completedAtProp.GetSetMethod(true)!.IsPrivate,
                "CompletedAt deve continuar com private set;");

            // Remarks
            var remarksProp = t.GetProperty("Remarks", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(remarksProp);
            Assert.Equal(typeof(string), remarksProp!.PropertyType); // string? em runtime é System.String
            Assert.NotNull(remarksProp.GetGetMethod());
            Assert.Null(remarksProp.GetSetMethod());
            Assert.True(remarksProp.GetSetMethod(true)!.IsPrivate,
                "Remarks deve continuar com private set;");
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
            Assert.True(privateCtor!.IsPrivate,
                "O construtor vazio deve continuar private. Se mudar, atualize o teste.");

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

            // UpdatedAt ainda pode ser null, porque construtor não chama Touch()
            // (Touch só aparece nos métodos mutadores).
            // Não vamos exigir UpdatedAt != null aqui.
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
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            exec.SetStep(99);

            Assert.Equal(99, exec.StepId);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetStep deve chamar Touch() e atualizar UpdatedAt.");
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
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            exec.SetUser(123);

            Assert.Equal(123, exec.UserId);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetUser deve chamar Touch() e atualizar UpdatedAt.");
        }

        [Fact]
        public void SetUser_ShouldAcceptNull_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 99);
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            exec.SetUser(null);

            Assert.Null(exec.UserId);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before);
        }

        // ------------------------------------------------------------------------------------------------
        // 5. SetStatus
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetStatus_ShouldUpdateStatus_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            exec.SetStatus(ExecutionStatus.Cancelado);

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetStatus deve chamar Touch().");
        }

        // ------------------------------------------------------------------------------------------------
        // 6. SetRemarks
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void SetRemarks_WithValidText_ShouldTrimAndUpdate_AndTouch()
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            exec.SetRemarks("   algum apontamento importante   ");

            Assert.Equal("algum apontamento importante", exec.Remarks);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetRemarks deve chamar Touch().");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void SetRemarks_WithNullOrWhitespace_ShouldBecomeNull_AndTouch(string? input)
        {
            var exec = new ProcessExecution(10, 20, 30);
            var before = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);
            exec.SetRemarks(input);

            Assert.Null(exec.Remarks);
            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetRemarks deve chamar Touch() mesmo limpando.");
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

            var before = exec.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            exec.SetTimes(newStart, newEnd);

            Assert.Equal(newStart, exec.StartedAt);
            Assert.Equal(newEnd, exec.CompletedAt);

            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before,
                "SetTimes deve chamar Touch().");
        }

        [Fact]
        public void SetTimes_ShouldAcceptNulls()
        {
            var exec = new ProcessExecution(10, 20, 30);

            var before = exec.UpdatedAt;
            System.Threading.Thread.Sleep(5);

            exec.SetTimes(null, null);

            Assert.Null(exec.StartedAt);
            Assert.Null(exec.CompletedAt);

            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= before);
        }

        // ------------------------------------------------------------------------------------------------
        // 8. Start
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Start_ShouldSetStatusToIniciado_SetStartedAtUtcNow_ResetCompletedAt_AndTouch()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetStatus(ExecutionStatus.Pendente); // força outro estado inicial
            exec.SetTimes(DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1)); // simula já ter completado
            exec.SetRemarks("antigo");

            var beforeUtc = DateTime.UtcNow;
            var beforeUpdatedAt = exec.UpdatedAt;

            System.Threading.Thread.Sleep(5);

            // Act
            exec.Start(userId: 777);

            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal(777, exec.UserId);
            Assert.Equal(ExecutionStatus.Iniciado, exec.Status);

            Assert.NotNull(exec.StartedAt);
            Assert.True(exec.StartedAt >= beforeUtc && exec.StartedAt <= afterUtc,
                "Start() deve atualizar StartedAt com UtcNow.");

            Assert.Null(exec.CompletedAt);

            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= beforeUpdatedAt,
                "Start() deve chamar Touch().");
        }

        [Fact]
        public void Start_ShouldNotOverrideUserId_WhenUserIdParameterIsNull()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30); // já tem UserId=30
            var beforeUser = exec.UserId;

            // Act
            exec.Start(userId: null);

            // Assert
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
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks(" já tinha observação ");
            var beforeUpdatedAt = exec.UpdatedAt;
            var beforeUtc = DateTime.UtcNow;

            System.Threading.Thread.Sleep(5);

            // Act
            exec.Complete("   finalizado sem pendências   ");

            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal(ExecutionStatus.Concluido, exec.Status);

            Assert.NotNull(exec.CompletedAt);
            Assert.True(exec.CompletedAt >= beforeUtc && exec.CompletedAt <= afterUtc,
                "Complete() deve definir CompletedAt = UtcNow.");

            Assert.Equal("finalizado sem pendências", exec.Remarks);

            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= beforeUpdatedAt,
                "Complete() deve chamar Touch().");
        }

        [Fact]
        public void Complete_WithNullRemarks_ShouldKeepExistingRemarks()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            // Act
            exec.Complete(null);

            // Assert
            Assert.Equal(ExecutionStatus.Concluido, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks); // mantém
            Assert.NotNull(exec.CompletedAt);
        }

        [Fact]
        public void Complete_WithWhitespaceRemarks_ShouldKeepExistingRemarks()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            // Act
            exec.Complete("     ");

            // Assert
            Assert.Equal(ExecutionStatus.Concluido, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks); // mantém
            Assert.NotNull(exec.CompletedAt);
        }

        // ------------------------------------------------------------------------------------------------
        // 10. Cancel
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void Cancel_ShouldSetStatusToCancelado_SetCompletedAtUtcNow_KeepOrUpdateRemarks_AndTouch()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("chamado reaberto por falha");
            var beforeUpdatedAt = exec.UpdatedAt;
            var beforeUtc = DateTime.UtcNow;

            System.Threading.Thread.Sleep(5);

            // Act
            exec.Cancel("   cancelamento solicitado pelo cliente   ");

            var afterUtc = DateTime.UtcNow;

            // Assert
            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);

            Assert.NotNull(exec.CompletedAt);
            Assert.True(exec.CompletedAt >= beforeUtc && exec.CompletedAt <= afterUtc,
                "Cancel() deve definir CompletedAt = UtcNow.");

            Assert.Equal("cancelamento solicitado pelo cliente", exec.Remarks);

            Assert.NotNull(exec.UpdatedAt);
            Assert.True(exec.UpdatedAt >= beforeUpdatedAt,
                "Cancel() deve chamar Touch().");
        }

        [Fact]
        public void Cancel_WithNullRemarks_ShouldKeepExistingRemarks()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            // Act
            exec.Cancel(null);

            // Assert
            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks); // mantém
            Assert.NotNull(exec.CompletedAt);
        }

        [Fact]
        public void Cancel_WithWhitespaceRemarks_ShouldKeepExistingRemarks()
        {
            // Arrange
            var exec = new ProcessExecution(10, 20, 30);
            exec.SetRemarks("observação inicial");

            // Act
            exec.Cancel("     ");

            // Assert
            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.Equal("observação inicial", exec.Remarks); // mantém
            Assert.NotNull(exec.CompletedAt);
        }

        // ------------------------------------------------------------------------------------------------
        // 11. MUTATION STABILITY / TOUCH()
        //     Garantir que todos os métodos mutadores não explodam e atualizam UpdatedAt.
        // ------------------------------------------------------------------------------------------------

        [Fact]
        public void AllMutationMethods_ShouldNotThrow_And_ShouldUpdateUpdatedAt()
        {
            var exec = new ProcessExecution(10, 20, 30);

            var ex1 = Record.Exception(() => exec.SetStep(77));
            var after1 = exec.UpdatedAt;

            var ex2 = Record.Exception(() => exec.SetUser(88));
            var after2 = exec.UpdatedAt;

            var ex3 = Record.Exception(() => exec.SetStatus(ExecutionStatus.Pendente));
            var after3 = exec.UpdatedAt;

            var ex4 = Record.Exception(() => exec.SetRemarks("teste touch"));
            var after4 = exec.UpdatedAt;

            var ex5 = Record.Exception(() => exec.SetTimes(DateTime.UtcNow, DateTime.UtcNow));
            var after5 = exec.UpdatedAt;

            var ex6 = Record.Exception(() => exec.Start(userId: 123));
            var after6 = exec.UpdatedAt;

            var ex7 = Record.Exception(() => exec.Complete("done"));
            var after7 = exec.UpdatedAt;

            var ex8 = Record.Exception(() => exec.Cancel("cancel"));
            var after8 = exec.UpdatedAt;

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
            Assert.Null(ex5);
            Assert.Null(ex6);
            Assert.Null(ex7);
            Assert.Null(ex8);

            Assert.NotNull(after1);
            Assert.NotNull(after2);
            Assert.NotNull(after3);
            Assert.NotNull(after4);
            Assert.NotNull(after5);
            Assert.NotNull(after6);
            Assert.NotNull(after7);
            Assert.NotNull(after8);

            Assert.True(after2 >= after1);
            Assert.True(after3 >= after2);
            Assert.True(after4 >= after3);
            Assert.True(after5 >= after4);
            Assert.True(after6 >= after5);
            Assert.True(after7 >= after6);
            Assert.True(after8 >= after7);
        }
    }
}

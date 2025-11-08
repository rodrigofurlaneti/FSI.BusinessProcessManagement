using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Enums;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Services;
using Moq;
using Xunit;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;
using static FSI.BusinessProcessManagement.UnitTests.Domain.Helpers.ProcessServiceHelper;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Services
{
    public class ProcessServiceTests
    {
        // ------------------------- CreateProcessAsync -------------------------

        [Fact]
        public async Task CreateProcessAsync_Happy_NoDept_NoCreator_ShouldInsertAndCommit()
        {
            var uow = NewUowMock(out var dep, out var usr, out var rol, out var proc, out var stepRepo, out var execRepo);

            DomainProcess? captured = null;
            proc.Setup(r => r.InsertAsync(It.IsAny<DomainProcess>()))
                .Callback<DomainProcess>(p => captured = p)
                .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            var result = await sut.CreateProcessAsync("  Fluxo  ", null, " desc ", null);

            Assert.NotNull(result);
            Assert.Same(captured, result);
            Assert.Equal("Fluxo", result.Name);
            Assert.Equal("desc", result.Description);
            Assert.Null(result.DepartmentId);
            Assert.Null(result.CreatedBy);

            dep.Verify(d => d.GetByIdAsync(It.IsAny<long>()), Times.Never);
            usr.Verify(d => d.GetByIdAsync(It.IsAny<long>()), Times.Never);
            proc.Verify(r => r.InsertAsync(It.IsAny<DomainProcess>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateProcessAsync_InvalidName_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out _);
            var sut = new ProcessService(uow.Object);

            await Assert.ThrowsAsync<DomainException>(() =>
                sut.CreateProcessAsync("   ", null, null, null));
        }

        [Fact]
        public async Task CreateProcessAsync_DepartmentNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out var dep, out var usr, out var rol, out var proc, out var stepRepo, out var execRepo);

            dep.Setup(r => r.GetByIdAsync(10)).ReturnsAsync((Department?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.CreateProcessAsync("Proc", departmentId: 10));

            Assert.Equal("Department not found.", ex.Message);

            dep.Verify(r => r.GetByIdAsync(10), Times.Once);
            proc.Verify(r => r.InsertAsync(It.IsAny<DomainProcess>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task CreateProcessAsync_CreatedByNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out var dep, out var usr, out var rol, out var proc, out var stepRepo, out var execRepo);

            dep.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateWithPrivateCtor<Department>());
            usr.Setup(r => r.GetByIdAsync(7)).ReturnsAsync((User?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.CreateProcessAsync("Proc", departmentId: 1, createdBy: 7));

            Assert.Equal("CreatedBy user not found.", ex.Message);

            dep.Verify(r => r.GetByIdAsync(1), Times.Once);
            usr.Verify(r => r.GetByIdAsync(7), Times.Once);
            proc.Verify(r => r.InsertAsync(It.IsAny<DomainProcess>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        // ------------------------- AddStepAsync -------------------------

        [Fact]
        public async Task AddStepAsync_Happy_ShouldInsertAndCommit()
        {
            var uow = NewUowMock(out _, out _, out var rol, out var proc, out var steps, out var execs);

            var process = new DomainProcess("P1");
            SetId(process, 100);

            proc.Setup(r => r.GetByIdAsync(100)).ReturnsAsync(process);
            steps.Setup(r => r.GetByProcessIdAsync(100)).ReturnsAsync(new List<ProcessStep>());

            ProcessStep? captured = null;
            steps.Setup(r => r.InsertAsync(It.IsAny<ProcessStep>()))
                 .Callback<ProcessStep>(s => captured = s)
                 .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            var result = await sut.AddStepAsync(100, "  Validar  ", 1, assignedRoleId: null);

            Assert.NotNull(result);
            Assert.Same(captured, result);
            Assert.Equal(100, result.ProcessId);
            Assert.Equal(1, result.StepOrder);
            Assert.Equal("Validar", result.StepName);

            // ✅ verifique TODAS as chamadas que realmente acontecem
            proc.Verify(r => r.GetByIdAsync(100), Times.Once);
            steps.Verify(r => r.GetByProcessIdAsync(100), Times.Once);

            steps.Verify(r => r.InsertAsync(It.IsAny<ProcessStep>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);

            // opcional: agora passa sem erro
            uow.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task AddStepAsync_InvalidName_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out _);
            var sut = new ProcessService(uow.Object);

            await Assert.ThrowsAsync<DomainException>(() =>
                sut.AddStepAsync(1, "   ", 1, null));
        }

        [Fact]
        public async Task AddStepAsync_ProcessNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out var proc, out var steps, out _);

            proc.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DomainProcess?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.AddStepAsync(1, "A", 1, null));

            Assert.Equal("Process not found.", ex.Message);

            proc.Verify(r => r.GetByIdAsync(1), Times.Once);
            steps.Verify(r => r.InsertAsync(It.IsAny<ProcessStep>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
            uow.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task AddStepAsync_RoleNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out var rol, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 2);

            proc.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(process);
            rol.Setup(r => r.GetByIdAsync(9)).ReturnsAsync((Role?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.AddStepAsync(2, "A", 1, assignedRoleId: 9));

            Assert.Equal("Role not found.", ex.Message);

            proc.Verify(r => r.GetByIdAsync(2), Times.Once);
            rol.Verify(r => r.GetByIdAsync(9), Times.Once);

            steps.Verify(r => r.GetByProcessIdAsync(It.IsAny<long>()), Times.Never);
            steps.Verify(r => r.InsertAsync(It.IsAny<ProcessStep>()), Times.Never);

            uow.Verify(x => x.CommitAsync(), Times.Never);

            uow.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task AddStepAsync_DuplicateOrder_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out var rol, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 3);

            var s1 = new ProcessStep(3, "Etapa 1", 1, null);

            steps.Setup(r => r.GetByProcessIdAsync(3))
                 .ReturnsAsync(new List<ProcessStep> { s1 });

            proc.Setup(r => r.GetByIdAsync(3))
                .ReturnsAsync(process);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.AddStepAsync(3, "Outra", 1, null));

            Assert.Equal("There is already a step with order 1 in this process.", ex.Message);

            proc.Verify(r => r.GetByIdAsync(3), Times.Once);
            steps.Verify(r => r.GetByProcessIdAsync(3), Times.Once);

            steps.Verify(r => r.InsertAsync(It.IsAny<ProcessStep>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);

            uow.VerifyNoOtherCalls();
        }


        // ------------------------- StartExecutionAsync -------------------------

        [Fact]
        public async Task StartExecutionAsync_Happy_ShouldInsertAndCommit()
        {
            var uow = NewUowMock(out _, out var usr, out _, out var proc, out var steps, out var execs);

            var process = new DomainProcess("Proc");
            SetId(process, 10);

            var step = new ProcessStep(10, "S1", 1, null);
            SetId(step, 1001);

            proc.Setup(r => r.GetByIdAsync(10)).ReturnsAsync(process);
            steps.Setup(r => r.GetByIdAsync(1001)).ReturnsAsync(step);

            ProcessExecution? captured = null;
            execs.Setup(r => r.InsertAsync(It.IsAny<ProcessExecution>()))
                 .Callback<ProcessExecution>(e => captured = e)
                 .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            var result = await sut.StartExecutionAsync(10, 1001, userId: null);

            Assert.NotNull(result);
            Assert.Same(captured, result);
            Assert.Equal(10, result.ProcessId);
            Assert.Equal(1001, result.StepId);
            Assert.Null(result.CompletedAt);
            Assert.Equal(ExecutionStatus.Iniciado, result.Status);

            proc.Verify(r => r.GetByIdAsync(10), Times.Once);
            steps.Verify(r => r.GetByIdAsync(1001), Times.Once);
            usr.Verify(r => r.GetByIdAsync(It.IsAny<long>()), Times.Never); // userId nulo ⇒ sem lookup

            execs.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);

            uow.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task StartExecutionAsync_ProcessNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out var proc, out var steps, out var execs);

            proc.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((DomainProcess?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.StartExecutionAsync(1, 2, null));

            Assert.Equal("Process not found.", ex.Message);

            execs.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
        }

        [Fact]
        public async Task StartExecutionAsync_StepNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 5);
            proc.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(process);

            steps.Setup(r => r.GetByIdAsync(50)).ReturnsAsync((ProcessStep?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.StartExecutionAsync(5, 50, null));

            Assert.Equal("Step not found.", ex.Message);
        }

        [Fact]
        public async Task StartExecutionAsync_StepFromAnotherProcess_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 7);
            proc.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(process);

            var step = new ProcessStep(999, "S", 1, null); // processId diferente
            SetId(step, 70);
            steps.Setup(r => r.GetByIdAsync(70)).ReturnsAsync(step);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.StartExecutionAsync(7, 70, null));

            Assert.Equal("Step does not belong to the informed process.", ex.Message);
        }

        [Fact]
        public async Task StartExecutionAsync_UserNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out var usr, out _, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 8);
            proc.Setup(r => r.GetByIdAsync(8)).ReturnsAsync(process);

            var step = new ProcessStep(8, "S", 1, null);
            SetId(step, 800);
            steps.Setup(r => r.GetByIdAsync(800)).ReturnsAsync(step);

            usr.Setup(r => r.GetByIdAsync(321)).ReturnsAsync((User?)null);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.StartExecutionAsync(8, 800, userId: 321));

            Assert.Equal("User not found.", ex.Message);
        }

        [Fact]
        public async Task StartExecutionAsync_UserInactive_ShouldThrow()
        {
            var uow = NewUowMock(out _, out var usr, out _, out var proc, out var steps, out _);

            var process = new DomainProcess("P");
            SetId(process, 9);
            proc.Setup(r => r.GetByIdAsync(9)).ReturnsAsync(process);

            var step = new ProcessStep(9, "S", 1, null);
            SetId(step, 900);
            steps.Setup(r => r.GetByIdAsync(900)).ReturnsAsync(step);

            var user = new User("u", "h", isActive: false);
            usr.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(user);

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.StartExecutionAsync(9, 900, userId: 2));

            Assert.Equal("User is inactive.", ex.Message);
        }

        // ------------------------- CompleteExecutionAsync / CancelExecutionAsync -------------------------

        [Fact]
        public async Task CompleteExecutionAsync_Happy_ShouldUpdateAndCommit()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out var execs);

            var exec = new ProcessExecution(10, 100, null);
            exec.SetRemarks("old");
            SetId(exec, 5000);

            execs.Setup(r => r.GetByIdAsync(5000)).ReturnsAsync(exec);
            execs.Setup(r => r.UpdateAsync(exec)).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            await sut.CompleteExecutionAsync(5000, " done ");

            Assert.Equal(ExecutionStatus.Concluido, exec.Status);
            Assert.Equal("done", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);

            // ✅ verifique todas as chamadas esperadas
            execs.Verify(r => r.GetByIdAsync(5000), Times.Once);
            execs.Verify(r => r.UpdateAsync(exec), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);

            // ✅ agora pode manter sem erro
            uow.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task CompleteExecutionAsync_NotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out var execs);
            execs.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProcessExecution?)null);

            var sut = new ProcessService(uow.Object);
            var ex = await Assert.ThrowsAsync<DomainException>(() => sut.CompleteExecutionAsync(1, null));

            Assert.Equal("Execution not found.", ex.Message);
        }

        [Fact]
        public async Task CancelExecutionAsync_Happy_ShouldUpdateAndCommit()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out var execs);

            var exec = new ProcessExecution(10, 100, null);
            SetId(exec, 6000);

            execs.Setup(r => r.GetByIdAsync(6000)).ReturnsAsync(exec);
            execs.Setup(r => r.UpdateAsync(exec)).Returns(Task.CompletedTask);
            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            await sut.CancelExecutionAsync(6000, "motivo");

            Assert.Equal(ExecutionStatus.Cancelado, exec.Status);
            Assert.Equal("motivo", exec.Remarks);
            Assert.NotNull(exec.CompletedAt);

            execs.Verify(r => r.UpdateAsync(exec), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task CancelExecutionAsync_NotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out _, out var execs);
            execs.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((ProcessExecution?)null);

            var sut = new ProcessService(uow.Object);
            var ex = await Assert.ThrowsAsync<DomainException>(() => sut.CancelExecutionAsync(1, null));

            Assert.Equal("Execution not found.", ex.Message);
        }

        // ------------------------- AdvanceToNextStepAsync -------------------------

        [Fact]
        public async Task AdvanceToNextStepAsync_WithNextStep_ShouldCompleteCurrent_CreateNext_AndCommit()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out var steps, out var execs);

            var current = new ProcessExecution(100, 1001, userId: null);
            SetId(current, 9000);

            execs.Setup(r => r.GetByIdAsync(9000)).ReturnsAsync(current);
            execs.Setup(r => r.UpdateAsync(current)).Returns(Task.CompletedTask);

            var s1 = new ProcessStep(100, "S1", 1, null); SetId(s1, 1001);
            var s2 = new ProcessStep(100, "S2", 2, null); SetId(s2, 1002);

            steps.Setup(r => r.GetByProcessIdAsync(100))
                 .ReturnsAsync(new List<ProcessStep> { s2, s1 }); // fora de ordem

            ProcessExecution? capturedNext = null;
            execs.Setup(r => r.InsertAsync(It.IsAny<ProcessExecution>()))
                 .Callback<ProcessExecution>(e => capturedNext = e)
                 .Returns(Task.CompletedTask);

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            var next = await sut.AdvanceToNextStepAsync(9000, userId: 777, completeRemarks: "ok");

            Assert.NotNull(next);
            Assert.Same(capturedNext, next);
            Assert.Equal(100, next!.ProcessId);
            Assert.Equal(1002, next.StepId);
            Assert.Equal(ExecutionStatus.Iniciado, next.Status);

            Assert.Equal(ExecutionStatus.Concluido, current.Status);
            Assert.NotNull(current.CompletedAt);
            Assert.Equal("ok", current.Remarks);

            // ✅ verifique as chamadas que de fato ocorrem
            execs.Verify(r => r.GetByIdAsync(9000), Times.Once);
            steps.Verify(r => r.GetByProcessIdAsync(100), Times.Once);

            execs.Verify(r => r.UpdateAsync(current), Times.Once);
            execs.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Once);
            uow.Verify(x => x.CommitAsync(), Times.Once);

            // opcional: agora passa sem erro
            uow.VerifyNoOtherCalls();
        }


        [Fact]
        public async Task AdvanceToNextStepAsync_NoNextStep_ShouldReturnNull_AndJustCommit()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out var steps, out var execs);

            var current = new ProcessExecution(100, 1002, null); // já no último
            SetId(current, 9100);

            execs.Setup(r => r.GetByIdAsync(9100)).ReturnsAsync(current);
            execs.Setup(r => r.UpdateAsync(current)).Returns(Task.CompletedTask);

            var s1 = new ProcessStep(100, "S1", 1, null); SetId(s1, 1001);
            var s2 = new ProcessStep(100, "S2", 2, null); SetId(s2, 1002);

            steps.Setup(r => r.GetByProcessIdAsync(100)).ReturnsAsync(new List<ProcessStep> { s1, s2 });

            uow.Setup(x => x.CommitAsync()).ReturnsAsync(1);

            var sut = new ProcessService(uow.Object);

            var next = await sut.AdvanceToNextStepAsync(9100, userId: null, completeRemarks: "fim");

            Assert.Null(next);
            Assert.Equal(ExecutionStatus.Concluido, current.Status);

            execs.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Once);
        }

        [Fact]
        public async Task AdvanceToNextStepAsync_CurrentStepNotFound_ShouldThrow()
        {
            var uow = NewUowMock(out _, out _, out _, out _, out var steps, out var execs);

            var current = new ProcessExecution(200, 9999, null); // stepId inexistente
            SetId(current, 9200);

            execs.Setup(r => r.GetByIdAsync(9200)).ReturnsAsync(current);
            execs.Setup(r => r.UpdateAsync(current)).Returns(Task.CompletedTask);

            var s1 = new ProcessStep(200, "S1", 1, null); SetId(s1, 2001);
            var s2 = new ProcessStep(200, "S2", 2, null); SetId(s2, 2002);

            steps.Setup(r => r.GetByProcessIdAsync(200)).ReturnsAsync(new List<ProcessStep> { s1, s2 });

            var sut = new ProcessService(uow.Object);

            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                sut.AdvanceToNextStepAsync(9200, userId: null, completeRemarks: null));

            Assert.Equal("Current step not found in process definition.", ex.Message);

            execs.Verify(r => r.InsertAsync(It.IsAny<ProcessExecution>()), Times.Never);
            uow.Verify(x => x.CommitAsync(), Times.Never);
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using FSI.BusinessProcessManagement.Domain.Entities;
using DomainProcess = FSI.BusinessProcessManagement.Domain.Entities.Process;

namespace FSI.BusinessProcessManagement.Domain.Services
{
    public class ProcessService
    {
        private readonly IUnitOfWork _uow;

        public ProcessService(IUnitOfWork uow)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        }

        public async Task<DomainProcess> CreateProcessAsync(string name, long? departmentId = null, string? description = null, long? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Process name is required.");

            if (departmentId.HasValue && await _uow.Departments.GetByIdAsync(departmentId.Value) is null)
                throw new DomainException("Department not found.");

            if (createdBy.HasValue && await _uow.Users.GetByIdAsync(createdBy.Value) is null)
                throw new DomainException("CreatedBy user not found.");

            var process = new DomainProcess(name.Trim(), departmentId, description, createdBy);
            await _uow.Processes.InsertAsync(process);
            await _uow.CommitAsync();
            return process;
        }

        public async Task<ProcessStep> AddStepAsync(long processId, string stepName, int stepOrder, long? assignedRoleId = null)
        {
            if (string.IsNullOrWhiteSpace(stepName))
                throw new DomainException("Step name is required.");

            var process = await _uow.Processes.GetByIdAsync(processId)
                ?? throw new DomainException("Process not found.");

            if (assignedRoleId.HasValue && await _uow.Roles.GetByIdAsync(assignedRoleId.Value) is null)
                throw new DomainException("Role not found.");

            var existingSteps = await _uow.ProcessSteps.GetByProcessIdAsync(processId);
            if (existingSteps.Any(s => s.StepOrder == stepOrder))
                throw new DomainException($"There is already a step with order {stepOrder} in this process.");

            var step = new ProcessStep(processId, stepName.Trim(), stepOrder, assignedRoleId);
            await _uow.ProcessSteps.InsertAsync(step);
            await _uow.CommitAsync();
            return step;
        }

        public async Task<ProcessExecution> StartExecutionAsync(long processId, long stepId, long? userId = null)
        {
            var process = await _uow.Processes.GetByIdAsync(processId)
                ?? throw new DomainException("Process not found.");

            var step = await _uow.ProcessSteps.GetByIdAsync(stepId)
                ?? throw new DomainException("Step not found.");

            if (step.ProcessId != processId)
                throw new DomainException("Step does not belong to the informed process.");

            if (userId.HasValue)
            {
                var user = await _uow.Users.GetByIdAsync(userId.Value)
                    ?? throw new DomainException("User not found.");
                if (!user.IsActive)
                    throw new DomainException("User is inactive.");
            }

            var execution = new ProcessExecution(processId, step.Id, userId);
            execution.Start(userId);

            await _uow.ProcessExecutions.InsertAsync(execution);
            await _uow.CommitAsync();
            return execution;
        }

        public async Task CompleteExecutionAsync(long executionId, string? remarks = null)
        {
            var exec = await _uow.ProcessExecutions.GetByIdAsync(executionId)
                ?? throw new DomainException("Execution not found.");

            exec.Complete(remarks);
            await _uow.ProcessExecutions.UpdateAsync(exec);
            await _uow.CommitAsync();
        }

        public async Task CancelExecutionAsync(long executionId, string? remarks = null)
        {
            var exec = await _uow.ProcessExecutions.GetByIdAsync(executionId)
                ?? throw new DomainException("Execution not found.");

            exec.Cancel(remarks);
            await _uow.ProcessExecutions.UpdateAsync(exec);
            await _uow.CommitAsync();
        }

        public async Task<ProcessExecution?> AdvanceToNextStepAsync(long currentExecutionId, long? userId = null, string? completeRemarks = null)
        {
            var current = await _uow.ProcessExecutions.GetByIdAsync(currentExecutionId)
                ?? throw new DomainException("Execution not found.");

            current.Complete(completeRemarks);
            await _uow.ProcessExecutions.UpdateAsync(current);

            var steps = (await _uow.ProcessSteps.GetByProcessIdAsync(current.ProcessId))
                .OrderBy(s => s.StepOrder)
                .ToList();

            var currentStep = steps.FirstOrDefault(s => s.Id == current.StepId)
                ?? throw new DomainException("Current step not found in process definition.");

            var next = steps.FirstOrDefault(s => s.StepOrder > currentStep.StepOrder);
            if (next == null)
            {
                await _uow.CommitAsync();
                return null;
            }

            var nextExec = new ProcessExecution(current.ProcessId, next.Id, userId);
            nextExec.Start(userId);

            await _uow.ProcessExecutions.InsertAsync(nextExec);
            await _uow.CommitAsync();
            return nextExec;
        }
    }
}

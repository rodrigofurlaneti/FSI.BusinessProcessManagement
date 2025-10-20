using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.Domain.ValueObjects
{
    public class ProcessService
    {
        private readonly IUnitOfWork _uow;
        public ProcessService(IUnitOfWork uow) => _uow = uow;


        public async Task<Process> CreateProcessAsync(string name, long? departmentId = null, string? description = null, long? createdBy = null)
        {
            if (departmentId.HasValue)
            {
                var dep = await _uow.Departments.GetByIdAsync(departmentId.Value);
                if (dep == null) throw new InvalidOperationException("Department not found.");
            }
            if (createdBy.HasValue)
            {
                var user = await _uow.Users.GetByIdAsync(createdBy.Value);
                if (user == null) throw new InvalidOperationException("CreatedBy user not found.");
            }
            var process = new Process(name, departmentId, description, createdBy);
            await _uow.Processes.InsertAsync(process);
            await _uow.CommitAsync();
            return process;
        }


        public async Task<ProcessStep> AddStepAsync(long processId, string stepName, int stepOrder, long? assignedRoleId = null)
        {
            var process = await _uow.Processes.GetByIdAsync(processId) ?? throw new InvalidOperationException("Process not found.");
            if (assignedRoleId.HasValue)
            {
                var role = await _uow.Roles.GetByIdAsync(assignedRoleId.Value);
                if (role == null) throw new InvalidOperationException("Role not found.");
            }
            var step = process.AddStep(stepName, stepOrder, assignedRoleId);
            await _uow.ProcessSteps.InsertAsync(step);
            await _uow.Processes.UpdateAsync(process);
            await _uow.CommitAsync();
            return step;
        }


        public async Task<ProcessExecution> StartExecutionAsync(long processId, long stepId, long? userId = null)
        {
            var process = await _uow.Processes.GetByIdAsync(processId) ?? throw new InvalidOperationException("Process not found.");
            if (!process.Steps.Any(s => s.StepId == stepId))
                throw new InvalidOperationException("Step not found in process.");
            if (userId.HasValue)
}
    }

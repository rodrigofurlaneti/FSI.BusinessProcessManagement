using FSI.BusinessProcessManagement.Domain.Exceptions;
namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public class Process : BaseEntity
    {
        private readonly List<ProcessStep> _steps = new();
        private readonly List<ProcessExecution> _executions = new();

        public string Name { get; private set; }
        public string? Description { get; private set; }
        public long? DepartmentId { get; private set; }
        public long? CreatedById { get; private set; }

        public IReadOnlyCollection<ProcessStep> Steps => _steps.AsReadOnly();
        public IReadOnlyCollection<ProcessExecution> Executions => _executions.AsReadOnly();

        public Process(string name, long? departmentId = null, string? description = null, long? createdBy = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Process name is required.");

            Name = name.Trim();
            DepartmentId = departmentId;
            Description = description?.Trim();
            CreatedById = createdBy;
        }

        public void SetDescription(string? description)
        {
            Description = description?.Trim();
            Touch();
        }

        public void SetDepartment(long? departmentId)
        {
            DepartmentId = departmentId;
            Touch();
        }

        public ProcessStep AddStep(string stepName, int stepOrder, long? assignedRoleId = null)
        {
            if (string.IsNullOrWhiteSpace(stepName))
                throw new DomainException("Step name is required.");

            if (_steps.Any(s => s.StepOrder == stepOrder))
                throw new DomainException($"A step with order {stepOrder} already exists for this process.");

            var step = new ProcessStep(this.Id, stepName.Trim(), stepOrder, assignedRoleId);
            _steps.Add(step);
            Touch();
            return step;
        }

        public void RemoveStep(long stepId)
        {
            var step = _steps.FirstOrDefault(s => s.Id == stepId || s.StepId == stepId);
            if (step == null)
                throw new DomainException("Step not found in process.");

            _steps.Remove(step);
            Touch();
        }

        public ProcessExecution StartExecution(long stepId, long? userId = null)
        {
            var step = _steps.FirstOrDefault(s => s.Id == stepId || s.StepId == stepId);
            if (step == null)
                throw new DomainException("Step not found in process.");

            var exec = new ProcessExecution(this.Id, step.StepId, userId);
            _executions.Add(exec);
            Touch();
            return exec;
        }
    }
}

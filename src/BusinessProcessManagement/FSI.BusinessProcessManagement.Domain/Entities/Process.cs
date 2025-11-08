using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class Process : BaseEntity
    {
        public string Name { get; private set; } = string.Empty;
        public long? DepartmentId { get; private set; }
        public string? Description { get; private set; }
        public long? CreatedBy { get; private set; }
        private readonly List<ProcessStep> _steps = new();
        private readonly ReadOnlyCollection<ProcessStep> _stepsView;

        public IReadOnlyList<ProcessStep> Steps => _stepsView;

        // EF
        private Process()
        {
            _stepsView = _steps.AsReadOnly();
        }

        public Process(string name, long? departmentId = null, string? description = null, long? createdBy = null)
        {
            _stepsView = _steps.AsReadOnly(); // cacheia a view uma única vez
            SetName(name);
            DepartmentId = departmentId;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            CreatedBy = createdBy;
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Process name is required.");
            if (name.Length > 200)
                throw new DomainException("Process name too long (max 200).");

            Name = name.Trim();
            Touch();
        }

        public void SetDepartment(long? departmentId)
        {
            DepartmentId = departmentId;
            Touch();
        }

        public void SetDescription(string? description)
        {
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            Touch();
        }

        public void SetCreatedBy(long? createdBy)
        {
            CreatedBy = createdBy;
            Touch();
        }

        public ProcessStep AddStep(string stepName, int stepOrder, long? assignedRoleId = null)
        {
            if (string.IsNullOrWhiteSpace(stepName))
                throw new DomainException("StepName is required.");
            if (_steps.Any(s => s.StepOrder == stepOrder))
                throw new DomainException($"A step with order {stepOrder} already exists.");

            // Atenção: se this.Id == 0 (entidade não persistida), o ctor de ProcessStep pode negar.
            var step = new ProcessStep(this.Id, stepName.Trim(), stepOrder, assignedRoleId);
            _steps.Add(step);
            Touch();
            return step;
        }

        public void RemoveStep(long stepId)
        {
            var s = _steps.FirstOrDefault(x => x.Id == stepId || x.StepId == stepId);
            if (s == null) throw new DomainException("Step not found.");
            _steps.Remove(s);
            Touch();
        }

        public ProcessExecution StartExecution(long stepId, long? userId = null)
        {
            if (stepId <= 0) throw new DomainException("StepId is invalid.");

            if (!_steps.Any(s => s.Id == stepId || s.StepId == stepId))
                 throw new DomainException("Step does not belong to this process.");

            var exec = new ProcessExecution(this.Id, stepId, userId);
            Touch();
            return exec;
        }
    }
}

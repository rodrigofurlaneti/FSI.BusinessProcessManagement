using System.Collections.Generic;
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
        public IReadOnlyCollection<ProcessStep> Steps => _steps;

        // Requisito do EF
        private Process() { }

        public Process(string name, long? departmentId = null, string? description = null, long? createdBy = null)
        {
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

        /// <summary>
        /// Inicia uma execução deste processo na etapa informada.
        /// (Regra de domínio: criação da execução parte do agregado Process)
        /// </summary>
        public ProcessExecution StartExecution(long stepId, long? userId = null)
        {
            if (stepId <= 0) throw new DomainException("StepId is invalid.");
            // (Opcional) você pode validar se o step pertence ao processo se a coleção Steps estiver carregada
            var exec = new ProcessExecution(this.Id, stepId, userId);
            Touch();
            return exec;
        }
    }
}

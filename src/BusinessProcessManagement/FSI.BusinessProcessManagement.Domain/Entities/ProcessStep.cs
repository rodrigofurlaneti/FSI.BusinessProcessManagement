using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class ProcessStep : BaseEntity
    {
        public long ProcessId { get; private set; }

        // Alias para compatibilidade com quem chama StepId:
        public long StepId => Id;

        public string StepName { get; private set; } = string.Empty;
        public int StepOrder { get; private set; }
        public long? AssignedRoleId { get; private set; }

        private ProcessStep() { }

        public ProcessStep(long processId, string stepName, int stepOrder, long? assignedRoleId)
        {
            if (processId <= 0) throw new DomainException("Invalid ProcessId.");
            ProcessId = processId;
            SetName(stepName);
            SetOrder(stepOrder);
            SetAssignedRole(assignedRoleId);
        }

        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("Step name is required.");
            StepName = name.Trim();
            Touch();
        }

        public void SetOrder(int order)
        {
            if (order < 0) throw new DomainException("Step order must be >= 0.");
            StepOrder = order;
            Touch();
        }

        public void SetAssignedRole(long? roleId)
        {
            if (roleId.HasValue && roleId.Value <= 0)
                throw new DomainException("Invalid RoleId.");
            AssignedRoleId = roleId;
            Touch();
        }
    }
}

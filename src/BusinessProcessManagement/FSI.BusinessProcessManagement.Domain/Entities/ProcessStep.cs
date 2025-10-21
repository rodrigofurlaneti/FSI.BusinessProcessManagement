using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    #region ProcessStep
    public sealed class ProcessStep : BaseEntity
    {
        public long StepId => Id;
        public long ProcessId { get; private set; }
        public string StepName { get; private set; }
        public int StepOrder { get; private set; }
        public long? AssignedRoleId { get; private set; }
        private ProcessStep() { }
        public ProcessStep(long processId, string stepName, int stepOrder, long? assignedRoleId = null)
        {
            if (processId <= 0) throw new DomainException("Invalid ProcessId for step.");
            if (string.IsNullOrWhiteSpace(stepName)) throw new DomainException("StepName is required.");
            if (stepOrder < 0) throw new DomainException("StepOrder must be >= 0.");
            ProcessId = processId;
            StepName = stepName.Trim();
            StepOrder = stepOrder;
            AssignedRoleId = assignedRoleId;
        }
        public void AssignRole(long? roleId)
        {
            AssignedRoleId = roleId;
            Touch();
        }
        public void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new DomainException("StepName is required.");
            StepName = name.Trim();
            Touch();
        }
        public void SetOrder(int order)
        {
            if (order < 0) throw new DomainException("StepOrder must be >= 0.");
            StepOrder = order;
            Touch();
        }
    }
    #endregion
}

using FSI.BusinessProcessManagement.Domain.Enums;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class ProcessExecution : Entity
    {
        public long ExecutionId => Id;
        public long ProcessId { get; private set; }
        public long StepId { get; private set; }
        public long? UserId { get; private set; }
        public ExecutionStatus Status { get; private set; }
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? Remarks { get; private set; }


        private ProcessExecution() { }


        public ProcessExecution(long processId, long stepId, long? userId = null)
        {
            if (processId <= 0) throw new DomainException("Invalid ProcessId.");
            if (stepId <= 0) throw new DomainException("Invalid StepId.");
            ProcessId = processId;
            StepId = stepId;
            UserId = userId;
            Status = ExecutionStatus.Pendente;
            CreatedAt = DateTime.UtcNow;
        }


        public void Start(long? userId = null)
        {
            if (Status != ExecutionStatus.Pendente)
                throw new DomainException("Only pending executions can be started.");
            UserId = userId ?? UserId;
            Status = ExecutionStatus.Iniciado;
            StartedAt = DateTime.UtcNow;
            Touch();
        }


        public void Complete(string? remarks = null)
        {
            if (Status == ExecutionStatus.Concluido) throw new DomainException("Execution already completed.");
            if (Status == ExecutionStatus.Cancelado) throw new DomainException("Cannot complete a cancelled execution.");
            Status = ExecutionStatus.Concluido;
            CompletedAt = DateTime.UtcNow;
            Remarks = remarks;
            Touch();
        }


        public void Cancel(string? remarks = null)
        {
            if (Status == ExecutionStatus.Concluido) throw new DomainException("Cannot cancel a completed execution.");
            if (Status == ExecutionStatus.Cancelado) throw new DomainException("Execution already cancelled.");
            Status = ExecutionStatus.Cancelado;
            CompletedAt = DateTime.UtcNow;
            Remarks = remarks;
            Touch();
        }


    }
}

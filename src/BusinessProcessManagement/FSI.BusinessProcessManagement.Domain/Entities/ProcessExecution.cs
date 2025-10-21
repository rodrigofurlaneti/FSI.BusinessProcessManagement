using System;
using FSI.BusinessProcessManagement.Domain.Enums;
using FSI.BusinessProcessManagement.Domain.Exceptions;

namespace FSI.BusinessProcessManagement.Domain.Entities
{
    public sealed class ProcessExecution : BaseEntity
    {
        public long ProcessId { get; private set; }
        public long StepId { get; private set; }
        public long? UserId { get; private set; }

        public ExecutionStatus Status { get; private set; } = ExecutionStatus.Pendente;
        public DateTime? StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public string? Remarks { get; private set; }

        private ProcessExecution() { }

        public ProcessExecution(long processId, long stepId, long? userId)
        {
            if (processId <= 0) throw new DomainException("Invalid ProcessId.");
            if (stepId <= 0) throw new DomainException("Invalid StepId.");

            ProcessId = processId;
            StepId = stepId;
            UserId = userId;
            Status = ExecutionStatus.Pendente;
            Touch();
        }

        // ===== Comportamentos (transições) =====

        public void Start()
        {
            Start(DateTime.UtcNow);
        }

        public void Start(DateTime? startedAt)
        {
            if (Status == ExecutionStatus.Concluido)
                throw new DomainException("Cannot start an already completed execution.");
            if (Status == ExecutionStatus.Cancelado)
                throw new DomainException("Cannot start a canceled execution.");

            Status = ExecutionStatus.Iniciado;
            StartedAt ??= startedAt ?? DateTime.UtcNow;
            Touch();
        }

        public void Complete()
        {
            Complete(DateTime.UtcNow, null);
        }

        public void Complete(string? remarks)
        {
            Complete(DateTime.UtcNow, remarks);
        }

        public void Complete(DateTime? completedAt, string? remarks = null)
        {
            if (Status == ExecutionStatus.Cancelado)
                throw new DomainException("Cannot complete a canceled execution.");

            if (!StartedAt.HasValue)
                StartedAt = DateTime.UtcNow; // inicia “on the fly” se ainda não tinha sido iniciado

            if (completedAt.HasValue && StartedAt.HasValue && completedAt < StartedAt)
                throw new DomainException("CompletedAt cannot be earlier than StartedAt.");

            CompletedAt = completedAt ?? DateTime.UtcNow;
            Status = ExecutionStatus.Concluido;
            SetRemarks(remarks);
            Touch();
        }

        public void Cancel()
        {
            Cancel(DateTime.UtcNow, null);
        }

        public void Cancel(string? remarks)
        {
            Cancel(DateTime.UtcNow, remarks);
        }

        public void Cancel(DateTime? at, string? remarks = null)
        {
            if (Status == ExecutionStatus.Concluido)
                throw new DomainException("Cannot cancel a completed execution.");

            // Usa CompletedAt como “data de término” também para cancelamento
            CompletedAt = at ?? DateTime.UtcNow;
            Status = ExecutionStatus.Cancelado;
            SetRemarks(remarks);
            Touch();
        }

        // ===== Métodos auxiliares (mantidos para compatibilidade com a camada Application) =====

        public void SetStatus(ExecutionStatus status)
        {
            // Evita voltar depois de concluído
            if (Status == ExecutionStatus.Concluido && status != ExecutionStatus.Concluido)
                throw new DomainException("Cannot change status after completion.");

            Status = status;
            Touch();
        }

        public void SetTimes(DateTime? startedAt, DateTime? completedAt)
        {
            if (completedAt.HasValue && startedAt.HasValue && completedAt < startedAt)
                throw new DomainException("CompletedAt cannot be earlier than StartedAt.");

            StartedAt = startedAt;
            CompletedAt = completedAt;
            Touch();
        }

        public void SetRemarks(string? remarks)
        {
            Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
            Touch();
        }
    }
}

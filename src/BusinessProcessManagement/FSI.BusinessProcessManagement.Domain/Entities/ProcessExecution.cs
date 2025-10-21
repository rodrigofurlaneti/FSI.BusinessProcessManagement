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

        // Requisito do EF
        private ProcessExecution() { }

        public ProcessExecution(long processId, long stepId, long? userId = null)
        {
            if (processId <= 0) throw new DomainException("ProcessId is invalid.");
            if (stepId <= 0) throw new DomainException("StepId is invalid.");

            ProcessId = processId;
            StepId = stepId;
            UserId = userId;

            // Ao criar já consideramos 'Iniciado' (ou troque para Pendente, se preferir)
            Status = ExecutionStatus.Iniciado;
            StartedAt = DateTime.UtcNow;
        }

        // --- Métodos para dar suporte aos mappers/app services ---

        public void SetStep(long stepId)
        {
            if (stepId <= 0) throw new DomainException("StepId is invalid.");
            StepId = stepId;
            Touch();
        }

        public void SetUser(long? userId)
        {
            UserId = userId;
            Touch();
        }

        public void SetStatus(ExecutionStatus status)
        {
            Status = status;
            Touch();
        }

        public void SetRemarks(string? remarks)
        {
            Remarks = string.IsNullOrWhiteSpace(remarks) ? null : remarks.Trim();
            Touch();
        }

        /// <summary>
        /// Ajuste direto dos timestamps (usado por mapper quando vier do DTO).
        /// </summary>
        public void SetTimes(DateTime? startedAtUtc, DateTime? completedAtUtc)
        {
            StartedAt = startedAtUtc;
            CompletedAt = completedAtUtc;
            Touch();
        }

        public void Start(long? userId = null)
        {
            UserId = userId ?? UserId;
            Status = ExecutionStatus.Iniciado;
            StartedAt = DateTime.UtcNow;
            CompletedAt = null;
            Touch();
        }

        public void Complete(string? remarks = null)
        {
            Status = ExecutionStatus.Concluido;
            Remarks = string.IsNullOrWhiteSpace(remarks) ? Remarks : remarks.Trim();
            CompletedAt = DateTime.UtcNow;
            Touch();
        }

        public void Cancel(string? remarks = null)
        {
            Status = ExecutionStatus.Cancelado;
            Remarks = string.IsNullOrWhiteSpace(remarks) ? Remarks : remarks.Trim();
            CompletedAt = DateTime.UtcNow;
            Touch();
        }
    }
}

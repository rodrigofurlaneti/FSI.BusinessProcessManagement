using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Enums;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class ProcessExecutionMapper
    {
        public static void CopyToExisting(ProcessExecution entity, ProcessExecutionDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Status) &&
                Enum.TryParse(dto.Status, true, out ExecutionStatus st))
            {
                entity.SetStatus(st);
            }
            entity.SetTimes(dto.StartedAt, dto.CompletedAt);
            entity.SetRemarks(dto.Remarks);
        }

        public static ProcessExecutionDto ToDto(ProcessExecution entity) => new()
        {
            ExecutionId = entity.Id,
            ProcessId = entity.ProcessId,
            StepId = entity.StepId,
            UserId = entity.UserId,
            Status = entity.Status.ToString(),
            StartedAt = entity.StartedAt,
            CompletedAt = entity.CompletedAt,
            Remarks = entity.Remarks
        };
    }
}

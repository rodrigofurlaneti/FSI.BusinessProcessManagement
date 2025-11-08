using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Mappers;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Mappers
{
    public class ProcessExecutionMapperTests
    {
        private static ExecutionStatus GetAnyStatus() =>
            Enum.GetValues(typeof(ExecutionStatus)).Cast<ExecutionStatus>().First();

        private static ExecutionStatus GetAnotherStatus(ExecutionStatus current)
        {
            var all = Enum.GetValues(typeof(ExecutionStatus)).Cast<ExecutionStatus>().ToArray();
            // tenta achar um diferente; se não houver, retorna o mesmo (raro, mas deixa o teste estável)
            return all.FirstOrDefault(s => !s.Equals(current), current);
        }

        private static ProcessExecution NewEntity(long processId = 100, long stepId = 10, long? userId = 1)
            => new ProcessExecution(processId, stepId, userId);

        [Fact]
        public void CopyToExisting_Should_Update_Status_When_DtoStatus_IsValid_CaseInsensitive()
        {
            // Arrange
            var entity = NewEntity();
            var initial = GetAnyStatus();
            entity.SetStatus(initial);

            var target = GetAnotherStatus(initial);
            var dto = new ProcessExecutionDto
            {
                // usa o nome do enum em minúsculas para validar o TryParse ignoreCase == true
                Status = target.ToString().ToLowerInvariant()
            };

            // Act
            ProcessExecutionMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(target, entity.Status);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("NOT_A_VALID_STATUS")]
        public void CopyToExisting_Should_Not_Change_Status_When_DtoStatus_IsNull_Whitespace_Or_Invalid(string? status)
        {
            // Arrange
            var entity = NewEntity();
            var initial = GetAnyStatus();
            entity.SetStatus(initial);

            var dto = new ProcessExecutionDto { Status = status };

            // Act
            ProcessExecutionMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(initial, entity.Status);
        }

        [Fact]
        public void CopyToExisting_Should_Update_Times_And_Remarks()
        {
            // Arrange
            var entity = NewEntity();
            var start = new DateTime(2025, 11, 8, 10, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2025, 11, 8, 11, 30, 0, DateTimeKind.Utc);

            var dto = new ProcessExecutionDto
            {
                StartedAt = start,
                CompletedAt = end,
                Remarks = "Execução atualizada via mapper"
            };

            // Act
            ProcessExecutionMapper.CopyToExisting(entity, dto);

            // Assert
            Assert.Equal(start, entity.StartedAt);
            Assert.Equal(end, entity.CompletedAt);
            Assert.Equal("Execução atualizada via mapper", entity.Remarks);
        }

        [Fact]
        public void ToDto_Should_Map_All_Fields()
        {
            // Arrange
            var entity = NewEntity(processId: 200, stepId: 20, userId: 99);
            entity.SetStatus(GetAnyStatus());
            var start = new DateTime(2025, 11, 8, 9, 15, 0, DateTimeKind.Utc);
            var end = new DateTime(2025, 11, 8, 12, 45, 0, DateTimeKind.Utc);
            entity.SetTimes(start, end);
            entity.SetRemarks("Observações X");

            var expectedId = entity.Id;
            var expectedStatusString = entity.Status.ToString();

            // Act
            var dto = ProcessExecutionMapper.ToDto(entity);

            // Assert
            Assert.NotNull(dto);
            Assert.Equal(expectedId, dto.ExecutionId);
            Assert.Equal(200, dto.ProcessId);
            Assert.Equal(20, dto.StepId);
            Assert.Equal(99, dto.UserId);
            Assert.Equal(expectedStatusString, dto.Status);
            Assert.Equal(start, dto.StartedAt);
            Assert.Equal(end, dto.CompletedAt);
            Assert.Equal("Observações X", dto.Remarks);
        }
    }
}
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Fakes;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public class AuditLogAppServiceContractTests
        : IGenericAppServiceContractTestsBase<AuditLogDto>
    {
        protected override IGenericAppService<AuditLogDto> CreateService()
        {
            long GetId(AuditLogDto dto) => dto.AuditId;
            void SetId(AuditLogDto dto, long id) => dto.AuditId = id;

            AuditLogDto ApplyUpdate(AuditLogDto current, AuditLogDto incoming)
            {
                current.UserId = incoming.UserId;
                current.ScreenId = incoming.ScreenId;
                current.ActionType = incoming.ActionType;
                current.ActionTimestamp = incoming.ActionTimestamp;
                current.AdditionalInfo = incoming.AdditionalInfo;
                return current;
            }

            return new InMemoryGenericAppService<AuditLogDto>(GetId, SetId, ApplyUpdate);
        }

        protected override AuditLogDto CreateNewDto() => new AuditLogDto
        {
            UserId = 10,
            ScreenId = 5,
            ActionType = "CREATE",
            ActionTimestamp = DateTime.UtcNow,
            AdditionalInfo = "Criado novo registro de teste"
        };

        protected override AuditLogDto CreateUpdatedDto(AuditLogDto original)
            => new AuditLogDto
            {
                AuditId = original.AuditId,
                UserId = original.UserId,
                ScreenId = original.ScreenId,
                ActionType = "UPDATE",
                ActionTimestamp = DateTime.UtcNow.AddMinutes(1),
                AdditionalInfo = "Atualização de registro"
            };

        protected override long GetId(AuditLogDto dto) => dto.AuditId;
    }
}

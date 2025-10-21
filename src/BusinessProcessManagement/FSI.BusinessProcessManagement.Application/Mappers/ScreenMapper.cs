using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Domain.Entities;

namespace FSI.BusinessProcessManagement.Application.Mappers
{
    public static class ScreenMapper
    {
        public static Screen ToNewEntity(ScreenDto dto)
            => new Screen(dto.ScreenName ?? string.Empty, dto.Description);

        public static void CopyToExisting(Screen entity, ScreenDto dto)
        {
            entity.SetName(dto.ScreenName ?? string.Empty);
            entity.SetDescription(dto.Description);
        }

        public static ScreenDto ToDto(Screen entity) => new()
        {
            ScreenId = entity.Id,
            ScreenName = entity.Name,
            Description = entity.Description
        };
    }
}

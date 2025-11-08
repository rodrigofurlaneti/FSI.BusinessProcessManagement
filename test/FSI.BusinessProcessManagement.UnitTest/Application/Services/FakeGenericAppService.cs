using FSI.BusinessProcessManagement.Application.Services;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Services
{
    public class FakeGenericAppService : GenericAppService<FakeDto, FakeEntity>
    {
        public FakeGenericAppService(IUnitOfWork uow, IRepository<FakeEntity> repo)
            : base(uow, repo) { }

        protected override FakeDto MapToDto(FakeEntity entity)
            => new FakeDto { Id = entity.Id, Name = entity.Name };

        protected override FakeEntity MapToEntity(FakeDto dto)
            => new FakeEntity { Id = dto.Id, Name = dto.Name };

        public override async Task UpdateAsync(FakeDto dto)
        {
            var entity = new FakeEntity { Id = dto.Id, Name = dto.Name + "_updated" };
            await Repository.UpdateAsync(entity);
            await Uow.CommitAsync();
        }
    }
}

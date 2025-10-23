using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;

namespace FSI.BusinessProcessManagement.Infrastructure.Persistence.Repositories
{
    public class ScreenRepository : GenericRepository<Screen>, IScreenRepository
    {
        public ScreenRepository(BpmDbContext bpmDbContext) : base(bpmDbContext) { }
    }
}

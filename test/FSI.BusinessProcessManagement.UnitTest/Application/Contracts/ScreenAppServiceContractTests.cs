using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public class ScreenAppServiceContractTests
        : IGenericAppServiceContractTestsBase<ScreenDto>
    {
        protected override IGenericAppService<ScreenDto> CreateService()
        {
            long GetId(ScreenDto d) => d.ScreenId;
            void SetId(ScreenDto d, long id) => d.ScreenId = id;

            ScreenDto ApplyUpdate(ScreenDto current, ScreenDto incoming)
            {
                current.ScreenName = incoming.ScreenName;
                current.Description = incoming.Description;
                return current;
            }

            return new InMemoryGenericAppService<ScreenDto>(GetId, SetId, ApplyUpdate);
        }

        protected override ScreenDto CreateNewDto() => new ScreenDto
        {
            ScreenName = "Dashboard",
            Description = "Tela inicial com KPIs"
        };

        protected override ScreenDto CreateUpdatedDto(ScreenDto original)
            => new ScreenDto
            {
                ScreenId = original.ScreenId, // manter o mesmo Id!
                ScreenName = "Relatórios",
                Description = "Tela de relatórios e análises"
            };

        protected override long GetId(ScreenDto dto) => dto.ScreenId;
    }
}
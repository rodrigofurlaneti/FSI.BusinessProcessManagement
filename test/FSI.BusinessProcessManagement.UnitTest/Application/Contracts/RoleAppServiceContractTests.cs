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
    public class RoleAppServiceContractTests
        : IGenericAppServiceContractTestsBase<RoleDto>
    {
        protected override IGenericAppService<RoleDto> CreateService()
        {
            long GetId(RoleDto d) => d.RoleId;
            void SetId(RoleDto d, long id) => d.RoleId = id;

            RoleDto ApplyUpdate(RoleDto current, RoleDto incoming)
            {
                current.RoleName = incoming.RoleName;
                current.Description = incoming.Description;
                return current;
            }

            return new InMemoryGenericAppService<RoleDto>(GetId, SetId, ApplyUpdate);
        }

        protected override RoleDto CreateNewDto() => new RoleDto
        {
            RoleName = "Administrador",
            Description = "Acesso total ao sistema"
        };

        protected override RoleDto CreateUpdatedDto(RoleDto original)
            => new RoleDto
            {
                RoleId = original.RoleId, // manter o mesmo Id!
                RoleName = "Gerente",
                Description = "Acesso a relatórios e cadastros"
            };

        protected override long GetId(RoleDto dto) => dto.RoleId;
    }
}
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
    public class UserRoleAppServiceContractTests
        : IGenericAppServiceContractTestsBase<UserRoleDto>
    {
        protected override IGenericAppService<UserRoleDto> CreateService()
        {
            long GetId(UserRoleDto d) => d.UserRoleId;
            void SetId(UserRoleDto d, long id) => d.UserRoleId = id;

            UserRoleDto ApplyUpdate(UserRoleDto current, UserRoleDto incoming)
            {
                current.UserId = incoming.UserId;
                current.RoleId = incoming.RoleId;
                return current;
            }

            return new InMemoryGenericAppService<UserRoleDto>(GetId, SetId, ApplyUpdate);
        }

        protected override UserRoleDto CreateNewDto() => new UserRoleDto
        {
            UserId = 100,
            RoleId = 200
        };

        protected override UserRoleDto CreateUpdatedDto(UserRoleDto original)
            => new UserRoleDto
            {
                UserRoleId = original.UserRoleId, // manter o mesmo Id!
                UserId = 101,  // simular mudança de vínculo
                RoleId = 201
            };

        protected override long GetId(UserRoleDto dto) => dto.UserRoleId;
    }
}
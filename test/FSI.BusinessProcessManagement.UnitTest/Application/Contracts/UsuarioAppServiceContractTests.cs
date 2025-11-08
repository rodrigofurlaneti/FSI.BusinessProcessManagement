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
    public class UsuarioAppServiceContractTests
        : IGenericAppServiceContractTestsBase<UsuarioDto>
    {
        protected override IGenericAppService<UsuarioDto> CreateService()
        {
            long GetId(UsuarioDto d) => d.UserId;
            void SetId(UsuarioDto d, long id) => d.UserId = id;

            UsuarioDto ApplyUpdate(UsuarioDto current, UsuarioDto incoming)
            {
                // mantém o Id e aplica mudanças relevantes
                current.DepartmentId = incoming.DepartmentId;
                current.Username = incoming.Username;     // (tem default string.Empty)
                current.PasswordHash = incoming.PasswordHash;
                current.Email = incoming.Email;
                current.IsActive = incoming.IsActive;     // (tem default true)
                return current;
            }

            return new InMemoryGenericAppService<UsuarioDto>(GetId, SetId, ApplyUpdate);
        }

        protected override UsuarioDto CreateNewDto() => new UsuarioDto
        {
            DepartmentId = 10,
            Username = "rfurlaneti",
            PasswordHash = "hash-inicial",
            Email = "rodrigo.furlaneti@empresa.com",
            IsActive = true
        };

        protected override UsuarioDto CreateUpdatedDto(UsuarioDto original)
            => new UsuarioDto
            {
                UserId = original.UserId, // manter o mesmo Id!
                DepartmentId = 20,
                Username = "rfurlaneti.atualizado",
                PasswordHash = "hash-atualizado",
                Email = "rodrigo.furlaneti+atualizado@empresa.com",
                IsActive = false
            };

        protected override long GetId(UsuarioDto dto) => dto.UserId;
    }
}
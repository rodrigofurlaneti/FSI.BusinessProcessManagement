using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Fakes;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Contracts
{
    public class DepartmentAppServiceContractTests
        : IGenericAppServiceContractTestsBase<DepartmentDto>
    {
        protected override IGenericAppService<DepartmentDto> CreateService()
        {
            long GetId(DepartmentDto d) => d.DepartmentId;
            void SetId(DepartmentDto d, long id) => d.DepartmentId = id;

            DepartmentDto ApplyUpdate(DepartmentDto current, DepartmentDto incoming)
            {
                current.DepartmentName = incoming.DepartmentName;
                current.Description = incoming.Description;
                return current;
            }

            return new InMemoryGenericAppService<DepartmentDto>(GetId, SetId, ApplyUpdate);
        }

        protected override DepartmentDto CreateNewDto() => new DepartmentDto
        {
            DepartmentName = "Tecnologia",
            Description = "Responsável por sistemas e infraestrutura"
        };

        protected override DepartmentDto CreateUpdatedDto(DepartmentDto original)
            => new DepartmentDto
            {
                DepartmentId = original.DepartmentId, // manter o mesmo Id!
                DepartmentName = "TI & Inovação",
                Description = "Sistemas, infraestrutura e inovação"
            };

        protected override long GetId(DepartmentDto dto) => dto.DepartmentId;
    }
}
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Interfaces;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Interfaces
{
    public class IRoleAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IRoleAppService),
                expectedName: nameof(IRoleAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_RoleDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IRoleAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(RoleDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IRoleAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Confere a constraint 'where TDto : class'
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Confere as assinaturas CRUD do tipo fechado IGenericAppService<RoleDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(RoleDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(RoleDto));
        }
    }
}
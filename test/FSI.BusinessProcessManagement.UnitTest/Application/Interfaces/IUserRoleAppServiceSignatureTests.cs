using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IUserRoleAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IUserRoleAppService),
                expectedName: nameof(IUserRoleAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_UserRoleDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IUserRoleAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(UserRoleDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IUserRoleAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Garante a constraint 'where TDto : class'
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Garante as assinaturas CRUD corretas para IGenericAppService<UserRoleDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(UserRoleDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(UserRoleDto));
        }
    }
}

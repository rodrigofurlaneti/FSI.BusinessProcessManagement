using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Interfaces;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Interfaces
{
    public class IRoleScreenPermissionAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IRoleScreenPermissionAppService),
                expectedName: nameof(IRoleScreenPermissionAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_RoleScreenPermissionDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IRoleScreenPermissionAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(RoleScreenPermissionDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IRoleScreenPermissionAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Verifica a constraint 'where TDto : class'
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Verifica as assinaturas CRUD para o tipo fechado IGenericAppService<RoleScreenPermissionDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(RoleScreenPermissionDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(RoleScreenPermissionDto));
        }
    }
}
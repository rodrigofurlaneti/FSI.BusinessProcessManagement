using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IScreenAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IScreenAppService),
                expectedName: nameof(IScreenAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_ScreenDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IScreenAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(ScreenDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IScreenAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Constraint do genérico (where TDto : class)
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Assinaturas CRUD para o tipo fechado IGenericAppService<ScreenDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(ScreenDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(ScreenDto));
        }
    }
}

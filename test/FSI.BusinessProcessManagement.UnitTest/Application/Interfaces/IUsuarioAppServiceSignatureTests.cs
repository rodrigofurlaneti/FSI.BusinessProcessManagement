using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IUsuarioAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IUsuarioAppService),
                expectedName: nameof(IUsuarioAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_UsuarioDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IUsuarioAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(UsuarioDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IUsuarioAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Constraint genérica (where TDto : class)
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Assinaturas CRUD corretas para IGenericAppService<UsuarioDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(UsuarioDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(UsuarioDto));
        }
    }
}

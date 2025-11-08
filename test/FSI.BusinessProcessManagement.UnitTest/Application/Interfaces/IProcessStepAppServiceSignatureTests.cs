using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IProcessStepAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IProcessStepAppService),
                expectedName: nameof(IProcessStepAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_ProcessStepDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IProcessStepAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(ProcessStepDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IProcessStepAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Verifica a constraint genérica (where TDto : class)
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Verifica assinaturas CRUD para IGenericAppService<ProcessStepDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(ProcessStepDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(ProcessStepDto));
        }
    }
}
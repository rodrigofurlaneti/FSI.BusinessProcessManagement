using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IProcessoBPMAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IProcessoBPMAppService),
                expectedName: nameof(IProcessoBPMAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_ProcessoBPMDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IProcessoBPMAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(ProcessoBPMDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IProcessoBPMAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Constraint do genérico (where TDto : class)
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Assinaturas CRUD para o tipo fechado IGenericAppService<ProcessoBPMDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(ProcessoBPMDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(ProcessoBPMDto));
        }
    }
}

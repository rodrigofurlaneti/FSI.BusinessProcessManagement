using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using FSI.BusinessProcessManagement.UnitTests.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Application.Interfaces
{
    public class IProcessExecutionAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IProcessExecutionAppService),
                expectedName: nameof(IProcessExecutionAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_ProcessExecutionDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IProcessExecutionAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(ProcessExecutionDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IProcessExecutionAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Constraint genérica (where TDto : class)
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Assinaturas CRUD para o tipo fechado IGenericAppService<ProcessExecutionDto>
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(ProcessExecutionDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(ProcessExecutionDto));
        }
    }
}
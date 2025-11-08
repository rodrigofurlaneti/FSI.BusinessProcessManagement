using System;
using FSI.BusinessProcessManagement.Application.Dtos;
using FSI.BusinessProcessManagement.Application.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    public class IDepartmentAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IDepartmentAppService),
                expectedName: nameof(IDepartmentAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_DepartmentDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IDepartmentAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(DepartmentDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IDepartmentAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            // Verifica constraint "where TDto : class"
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            // Verifica assinaturas dos métodos CRUD
            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(DepartmentDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(DepartmentDto));
        }
    }
}

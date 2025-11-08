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
    public class IAuditLogAppServiceSignatureTests
    {
        [Fact]
        public void Interface_Name_And_Namespace_Should_Be_Correct()
        {
            InterfaceSignatureAssert.IsInterfaceWithNameAndNamespace(
                typeof(IAuditLogAppService),
                expectedName: nameof(IAuditLogAppService),
                expectedNamespace: "FSI.BusinessProcessManagement.Application.Interfaces"
            );
        }

        [Fact]
        public void Should_Inherit_IGenericAppService_Of_AuditLogDto()
        {
            InterfaceSignatureAssert.InheritsGenericInterface(
                typeof(IAuditLogAppService),
                openGenericInterface: typeof(IGenericAppService<>),
                expectedGenericArgument: typeof(AuditLogDto)
            );
        }

        [Fact]
        public void Should_Not_Declare_Additional_Members()
        {
            InterfaceSignatureAssert.DeclaresNoAdditionalMembers(typeof(IAuditLogAppService));
        }

        [Fact]
        public void IGenericAppService_Should_Have_Class_Constraint_And_Correct_Methods()
        {
            InterfaceSignatureAssert.GenericParameterHasClassConstraint(typeof(IGenericAppService<>));

            var closed = typeof(IGenericAppService<>).MakeGenericType(typeof(AuditLogDto));
            InterfaceSignatureAssert.HasCrudMethodsWithCorrectSignatures(closed, typeof(AuditLogDto));
        }
    }
}
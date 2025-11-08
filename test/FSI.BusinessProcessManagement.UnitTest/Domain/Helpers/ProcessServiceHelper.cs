using System;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Moq;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Helpers
{
    public static class ProcessServiceHelper
    {
        /// <summary>Define o Id (protected set) em qualquer BaseEntity via reflexão.</summary>
        public static void SetId(object entity, long id)
        {
            var p = typeof(BaseEntity).GetProperty("Id",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            p!.SetValue(entity, id);
        }

        /// <summary>Instancia um tipo com construtor privado/padrão (compatível com entidades EF).</summary>
        public static T CreateWithPrivateCtor<T>() where T : class
        {
            var t = typeof(T);
            var ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                        binder: null, types: Type.EmptyTypes, modifiers: null);
            if (ctor != null) return (T)ctor.Invoke(null);
            throw new InvalidOperationException($"No parameterless ctor for {t.FullName}");
        }

        /// Cria um IUnitOfWork mockado (Strict) e retorna também os mocks dos repositórios.
        public static Mock<IUnitOfWork> NewUowMock(
            out Mock<IDepartmentRepository> departments,
            out Mock<IUserRepository> users,
            out Mock<IRoleRepository> roles,
            out Mock<IProcessRepository> processes,
            out Mock<IProcessStepRepository> steps,
            out Mock<IProcessExecutionRepository> executions)
        {
            var uow = new Mock<IUnitOfWork>(MockBehavior.Strict);

            departments = new Mock<IDepartmentRepository>(MockBehavior.Strict);
            users = new Mock<IUserRepository>(MockBehavior.Strict);
            roles = new Mock<IRoleRepository>(MockBehavior.Strict);
            processes = new Mock<IProcessRepository>(MockBehavior.Strict);
            steps = new Mock<IProcessStepRepository>(MockBehavior.Strict);
            executions = new Mock<IProcessExecutionRepository>(MockBehavior.Strict);

            uow.SetupGet(x => x.Departments).Returns(departments.Object);
            uow.SetupGet(x => x.Users).Returns(users.Object);
            uow.SetupGet(x => x.Roles).Returns(roles.Object);
            uow.SetupGet(x => x.Processes).Returns(processes.Object);
            uow.SetupGet(x => x.ProcessSteps).Returns(steps.Object);
            uow.SetupGet(x => x.ProcessExecutions).Returns(executions.Object);

            return uow;
        }
    }
}

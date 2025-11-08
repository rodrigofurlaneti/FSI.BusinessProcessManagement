using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IUnitOfWorkTests
    {
        // -------------------------------------------------------------------------
        // 1. Estrutura geral
        // -------------------------------------------------------------------------
        [Fact]
        public void IUnitOfWork_MustExist_AndBePublicInterface_AndInherit_IDisposable()
        {
            var t = typeof(IUnitOfWork);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IUnitOfWork deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IUnitOfWork deve continuar pública.");

            // Não pode ser classe (concreta ou abstrata)
            Assert.False(t.IsClass, "IUnitOfWork não pode ser classe.");

            // Interfaces não são sealed; manter esse guarda
            Assert.False(t.IsSealed, "Se virou sealed, quebrou o contrato.");

            // Deve herdar IDisposable
            Assert.Contains(typeof(IDisposable), t.GetInterfaces());
        }

        // -------------------------------------------------------------------------
        // 2. Propriedades de repositório expostas (somente get)
        // -------------------------------------------------------------------------
        [Fact]
        public void IUnitOfWork_MustExpose_AllExpectedRepositoryProperties_WithReadOnlyGetters()
        {
            var t = typeof(IUnitOfWork);

            var expectedProps = new (string Name, Type Type)[]
            {
                ("Departments", typeof(IDepartmentRepository)),
                ("Users", typeof(IUserRepository)),
                ("Roles", typeof(IRoleRepository)),
                ("UserRoles", typeof(IUserRoleRepository)),
                ("Screens", typeof(IScreenRepository)),
                ("RoleScreenPermissions", typeof(IRoleScreenPermissionRepository)),
                ("AuditLogs", typeof(IAuditLogRepository)),
                ("Processes", typeof(IProcessRepository)),
                ("ProcessSteps", typeof(IProcessStepRepository)),
                ("ProcessExecutions", typeof(IProcessExecutionRepository)),
            };

            foreach (var (propName, propType) in expectedProps)
            {
                var prop = t.GetProperty(propName, BindingFlags.Public | BindingFlags.Instance);

                Assert.NotNull(prop);
                Assert.Equal(propType, prop!.PropertyType);

                // deve ter getter público
                var getter = prop.GetGetMethod();
                Assert.NotNull(getter);
                Assert.True(getter!.IsPublic, $"{propName} deve ter get público.");

                // não deve ter setter público (corrigido: sem Assert.Null com mensagem)
                var setter = prop.GetSetMethod();
                Assert.True(setter == null, $"{propName} não deve ter set público.");
            }

            // Não deve haver propriedades extras/renomeadas
            var actualPropNames = t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                   .Select(p => p.Name)
                                   .OrderBy(n => n)
                                   .ToArray();

            var expectedPropNames = expectedProps.Select(p => p.Name)
                                                 .OrderBy(n => n)
                                                 .ToArray();

            Assert.Equal(expectedPropNames, actualPropNames);
        }

        // -------------------------------------------------------------------------
        // 3. Método CommitAsync
        // -------------------------------------------------------------------------
        [Fact]
        public void IUnitOfWork_MustDeclare_CommitAsync_Returning_TaskOfInt_WithNoParameters()
        {
            var t = typeof(IUnitOfWork);

            var method = t.GetMethod("CommitAsync", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(method);
            Assert.Equal("CommitAsync", method!.Name);

            // assinatura: Task<int> CommitAsync()
            Assert.Empty(method.GetParameters());
            Assert.True(method.ReturnType.IsGenericType, "CommitAsync deve retornar Task<int>.");
            Assert.Equal(typeof(Task<>), method.ReturnType.GetGenericTypeDefinition());
            Assert.Equal(typeof(int), method.ReturnType.GetGenericArguments().Single());
        }

        // -------------------------------------------------------------------------
        // 4. Não deve haver outros métodos públicos além do CommitAsync (fora os herdados)
        // -------------------------------------------------------------------------
        [Fact]
        public void IUnitOfWork_ShouldOnlyDeclare_CommitAsync_AsOwnPublicMethod()
        {
            var t = typeof(IUnitOfWork);

            var allMethods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                              .Where(m => !m.IsSpecialName)
                              .ToList();

            var inherited = t.GetInterfaces()
                             .SelectMany(i => i.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                             .Where(m => !m.IsSpecialName)
                             .Distinct()
                             .ToList();

            var declaredHere = allMethods
                .Where(m => !inherited.Any(bm =>
                    bm.Name == m.Name &&
                    bm.ReturnType == m.ReturnType &&
                    ParametersMatch(bm.GetParameters(), m.GetParameters())))
                .ToList();

            Assert.Single(declaredHere);
            var only = declaredHere[0];
            Assert.Equal("CommitAsync", only.Name);
            Assert.Empty(only.GetParameters());
            Assert.True(only.ReturnType.IsGenericType);
            Assert.Equal(typeof(Task<>), only.ReturnType.GetGenericTypeDefinition());
            Assert.Equal(typeof(int), only.ReturnType.GetGenericArguments().Single());
        }

        // -------------------------------------------------------------------------
        // 5. IDisposable / Dispose
        // -------------------------------------------------------------------------
        [Fact]
        public void IUnitOfWork_MustInherit_IDisposable_AndExpose_Dispose()
        {
            var t = typeof(IUnitOfWork);

            // herança
            Assert.True(typeof(IDisposable).IsAssignableFrom(t), "IUnitOfWork deve herdar IDisposable.");

            // assinatura de IDisposable.Dispose()
            var disposeBase = typeof(IDisposable).GetMethod("Dispose");
            Assert.NotNull(disposeBase);
            Assert.Equal(typeof(void), disposeBase!.ReturnType);
            Assert.Empty(disposeBase.GetParameters());

            // a interface pode NÃO declarar explicitamente Dispose (apenas herdar)
            var declaredDispose = t.GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);
            if (declaredDispose != null)
            {
                Assert.Equal(typeof(void), declaredDispose.ReturnType);
                Assert.Empty(declaredDispose.GetParameters());
            }
        }

        // helper
        private static bool ParametersMatch(ParameterInfo[] a, ParameterInfo[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i].ParameterType != b[i].ParameterType) return false;
                if (a[i].Name != b[i].Name) return false;
            }
            return true;
        }
    }
}

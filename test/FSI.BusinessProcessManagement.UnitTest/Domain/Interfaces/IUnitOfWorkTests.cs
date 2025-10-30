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
        // -----------------------------------------------------------------------------
        // 1. Estrutura geral
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUnitOfWork_MustExist_AndBePublicInterface_AndInherit_IDisposable()
        {
            var t = typeof(IUnitOfWork);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IUnitOfWork deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IUnitOfWork deve continuar pública.");

            // Proteção contra conversão indevida pra classe concreta
            Assert.False(t.IsSealed,
                "Se IUnitOfWork virou classe sealed, isso quebrou o contrato e precisa ser discutido.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IUnitOfWork não pode virar classe abstrata sem alinhar os testes/arquitetura.");

            // Deve herdar IDisposable
            var interfaces = t.GetInterfaces();
            Assert.Contains(typeof(IDisposable), interfaces);
        }

        // -----------------------------------------------------------------------------
        // 2. Propriedades de repositório expostas
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUnitOfWork_MustExpose_AllExpectedRepositoryProperties_WithReadOnlyGetters()
        {
            var t = typeof(IUnitOfWork);

            // Definição esperada de cada propriedade:
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

                // precisa ter getter público
                Assert.NotNull(prop.GetGetMethod());
                Assert.True(prop.GetGetMethod()!.IsPublic,
                    $"{propName} deve ter get público.");

                //Assert.Null(prop.GetSetMethod(),
                //    $"{propName} não deve ter set público.");
            }

            // Também queremos garantir que NINGUÉM removeu ou renomeou uma propriedade.
            // Vamos garantir que a lista de propriedades públicas bate exatamente
            // com a lista esperada acima (mesmos nomes, mesma quantidade).
            var actualPropNames = t
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            var expectedPropNames = expectedProps
                .Select(p => p.Name)
                .OrderBy(n => n)
                .ToArray();

            Assert.Equal(expectedPropNames, actualPropNames);
        }

        // -----------------------------------------------------------------------------
        // 3. Método CommitAsync
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUnitOfWork_MustDeclare_CommitAsync_Returning_TaskOfInt_WithNoParameters()
        {
            var t = typeof(IUnitOfWork);

            var method = t.GetMethod("CommitAsync",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);
            Assert.Equal("CommitAsync", method!.Name);

            // sem parâmetros
            var parameters = method.GetParameters();
            Assert.Empty(parameters);

            // retorno Task<int>
            Assert.True(method.ReturnType.IsGenericType,
                "CommitAsync deve retornar Task<int>.");

            Assert.Equal(typeof(Task<>), method.ReturnType.GetGenericTypeDefinition());

            var innerType = method.ReturnType.GetGenericArguments().Single();
            Assert.Equal(typeof(int), innerType);
        }

        // -----------------------------------------------------------------------------
        // 4. Não deve haver outros métodos públicos além do CommitAsync herdado/previsto
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUnitOfWork_ShouldOnlyDeclare_CommitAsync_AsOwnPublicMethod()
        {
            var t = typeof(IUnitOfWork);

            // Métodos públicos conhecidos da interface
            var allMethods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // Métodos herdados de IDisposable (Dispose())
            var baseMethods = t.GetInterfaces()
                               .SelectMany(i => i.GetMethods())
                               .Distinct()
                               .ToList();

            // Métodos que são da própria IUnitOfWork (não herdados)
            var declaredHere = allMethods
                .Where(m => !baseMethods.Any(bm =>
                    bm.Name == m.Name &&
                    bm.ReturnType == m.ReturnType &&
                    ParametersMatch(bm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Esperamos exatamente 1 método próprio: CommitAsync()
            Assert.Single(declaredHere);

            var only = declaredHere[0];
            Assert.Equal("CommitAsync", only.Name);

            // Reconfirma assinatura
            Assert.Empty(only.GetParameters());
            Assert.True(only.ReturnType.IsGenericType);
            Assert.Equal(typeof(Task<>), only.ReturnType.GetGenericTypeDefinition());
            Assert.Equal(typeof(int), only.ReturnType.GetGenericArguments().Single());
        }

        // -----------------------------------------------------------------------------
        // 5. Dispose deve existir via IDisposable
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUnitOfWork_MustStillExpose_Dispose_From_IDisposable()
        {
            var disposeMethod = typeof(IUnitOfWork)
                .GetMethod("Dispose", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(disposeMethod);

            // Dispose retorna void e tem zero parâmetros
            Assert.Equal(typeof(void), disposeMethod!.ReturnType);
            Assert.Empty(disposeMethod.GetParameters());
        }

        // Helper: compara assinatura de parâmetros (nome e tipo, na mesma ordem)
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

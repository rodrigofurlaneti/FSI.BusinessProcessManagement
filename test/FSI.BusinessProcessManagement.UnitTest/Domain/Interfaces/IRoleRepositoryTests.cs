using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IRoleRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IRoleRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IRoleRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IRoleRepository deve continuar pública.");

            // Não pode virar classe concreta ou sealed
            Assert.False(t.IsSealed,
                "Se IRoleRepository virou classe sealed, o contrato de domínio mudou e os testes precisam ser atualizados conscientemente.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IRoleRepository não pode virar classe abstrata sem revisão arquitetural.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<Role>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleRepository_MustInherit_IRepository_Of_Role()
        {
            var t = typeof(IRoleRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<Role>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(Role)
            );

            // Pega a interface IRepository<> herdada e valida o argumento genérico
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(Role), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Nenhum método declarado além dos herdados
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleRepository_ShouldNotDeclareExtraMethods_Yet()
        {
            var t = typeof(IRoleRepository);

            // Métodos visíveis na interface (inclui os herdados)
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<Role> e outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos que são especificamente declarados em IRoleRepository
            // (ou seja, que não existem nas interfaces pai)
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos ZERO métodos exclusivos.
            // Se no futuro você adicionar algo como:
            // Task<Role?> GetByNameAsync(string roleName);
            // => esse teste vai falhar e vai forçar o dev a atualizar este teste conscientemente.
            Assert.Empty(declaredHere);
        }

        // Helper pra comparar assinaturas de parâmetros (mesmo nome e tipo, na ordem)
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

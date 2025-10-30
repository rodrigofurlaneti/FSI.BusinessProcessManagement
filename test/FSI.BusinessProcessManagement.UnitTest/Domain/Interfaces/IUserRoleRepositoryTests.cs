using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IUserRoleRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUserRoleRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IUserRoleRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IUserRoleRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IUserRoleRepository deve continuar pública.");

            // proteção contra mudar pra classe concreta
            Assert.False(t.IsSealed,
                "Se IUserRoleRepository virou classe sealed, o contrato mudou e precisa ser revisado.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IUserRoleRepository não pode virar classe abstrata/concreta sem alinhar arquitetura.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<UserRole>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUserRoleRepository_MustInherit_IRepository_Of_UserRole()
        {
            var t = typeof(IUserRoleRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<UserRole>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(UserRole)
            );

            // Checagem mais explícita
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(UserRole), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Não deve declarar métodos próprios (ainda)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IUserRoleRepository_ShouldNotDeclareExtraMethods_Yet()
        {
            var t = typeof(IUserRoleRepository);

            // Todos os métodos que a interface expõe (inclui herdados)
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<UserRole> e outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos exclusivos de IUserRoleRepository
            // (ou seja, que não existem nas interfaces base)
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos ZERO métodos exclusivos.
            //
            // Se no futuro você adicionar, por exemplo:
            // Task<IEnumerable<UserRole>> GetByUserIdAsync(long userId);
            //
            // esse teste vai falhar e vai forçar o dev a atualizar conscientemente
            // (o que é exatamente a sua regra).
            Assert.Empty(declaredHere);
        }

        // Helper pra comparar parâmetros (tipo e nome, na mesma ordem)
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

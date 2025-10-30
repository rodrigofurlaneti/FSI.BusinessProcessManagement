using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IScreenRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IScreenRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IScreenRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IScreenRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IScreenRepository deve continuar pública.");

            // Proteção contra alteração indevida pra classe
            Assert.False(t.IsSealed,
                "Se IScreenRepository virou classe sealed, o contrato do domínio mudou e os testes precisam ser atualizados conscientemente.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IScreenRepository não pode virar classe abstrata/concreta sem alinhar arquitetura.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<Screen>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IScreenRepository_MustInherit_IRepository_Of_Screen()
        {
            var t = typeof(IScreenRepository);

            var interfaces = t.GetInterfaces();

            // Verifica se a interface herda IRepository<Screen>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(Screen)
            );

            // Validação detalhada do tipo genérico
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(Screen), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Não deve declarar métodos próprios (ainda)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IScreenRepository_ShouldNotDeclareExtraMethods_Yet()
        {
            var t = typeof(IScreenRepository);

            // Todos os métodos expostos pela interface (inclui os herdados de IRepository<>)
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<Screen> e outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos que são EXCLUSIVOS de IScreenRepository
            // (ou seja, não herdados)
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos ZERO métodos exclusivos.
            //
            // Se no futuro você decidir adicionar, por exemplo:
            //   Task<Screen?> GetByNameAsync(string name);
            // esse teste vai falhar e vai obrigar quem alterou a interface
            // a vir aqui e ajustar conscientemente.
            Assert.Empty(declaredHere);
        }

        // Helper: compara parâmetros (mesma quantidade, ordem, tipo e nome)
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

using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IDepartmentRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IDepartmentRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IDepartmentRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IDepartmentRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IDepartmentRepository deve ser pública.");
            Assert.False(t.IsSealed, "Se virou classe sealed, isso quebrou o contrato.");
            Assert.False(t.IsAbstract || t.IsClass,
                "IDepartmentRepository não pode virar classe abstrata ou concreta sem atualizar os testes.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<Department>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IDepartmentRepository_MustInherit_IRepository_Of_Department()
        {
            var t = typeof(IDepartmentRepository);

            // Interfaces herdadas diretamente (ex: IRepository<Department>)
            var interfaces = t.GetInterfaces();

            // Garante que entre as interfaces herdadas existe IRepository<Department>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(Department)
            );

            // Garante que realmente estamos herdando exatamente IRepository<T> com T = Department
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            // Verifica que tem exatamente um argumento genérico
            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);

            // Esse argumento deve ser Department
            Assert.Equal(typeof(Department), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Não deve definir métodos próprios (por enquanto)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IDepartmentRepository_ShouldNotDeclareExtraMethods_Yet()
        {
            var t = typeof(IDepartmentRepository);

            // Métodos diretamente declarados em IDepartmentRepository
            // (não herdados de IRepository<Department>).
            //
            // BindingFlags.Public | BindingFlags.Instance pega
            // assinaturas públicas da interface.
            //
            // GetMethods() em uma interface inclui métodos herdados também,
            // então precisamos filtrar pelos métodos que são *explicitamente*
            // declarados aqui, não herdados.
            //
            // A forma mais confiável: pegar todos e depois remover os que estão
            // declarados no primeiro nível das interfaces pai.
            var allMethods = t.GetMethods();
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Esperamos NENHUM método adicional hoje.
            // Se no futuro você adicionar, por exemplo:
            // Task<IEnumerable<Department>> GetActiveAsync();
            // esse teste vai falhar e o dev terá que atualizar conscientemente.
            Assert.Empty(declaredHere);
        }

        // Helper para comparar parâmetros por tipo e nome
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

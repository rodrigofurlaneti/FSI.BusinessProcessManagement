using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IProcessRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IProcessRepository);

            Assert.NotNull(t);
            Assert.True(t.IsInterface, "IProcessRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IProcessRepository deve continuar pública.");

            // Proteção contra mudança indevida para classe concreta
            Assert.False(t.IsSealed, "Interfaces não devem ser sealed; se virou classe sealed, contrato quebrou.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IProcessRepository não pode virar classe abstrata sem atualizar estes testes.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<Process>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessRepository_MustInherit_IRepository_Of_Process()
        {
            var t = typeof(IProcessRepository);

            var interfaces = t.GetInterfaces();

            // Confirmar que herda IRepository<Process>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(Process)
            );

            // Agora validar mais estritamente:
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            // IRepository<T>: deve haver exatamente 1 argumento genérico
            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);

            // Esse argumento deve ser Process
            Assert.Equal(typeof(Process), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Método GetByDepartmentAsync(departmentId)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessRepository_MustDeclare_GetByDepartmentAsync_WithExpectedSignature()
        {
            var t = typeof(IProcessRepository);

            var method = t.GetMethod("GetByDepartmentAsync",
                                     BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);
            Assert.Equal("GetByDepartmentAsync", method!.Name);

            // --- Parâmetros esperados ---
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var param = parameters[0];

            // Nome do parâmetro tem que continuar exatamente "departmentId"
            Assert.Equal("departmentId", param.Name);

            // Tipo do parâmetro tem que continuar exatamente long
            Assert.Equal(typeof(long), param.ParameterType);

            // --- Tipo de retorno esperado ---
            // Task<IEnumerable<Process>>
            var returnType = method.ReturnType;

            // Retorno externo deve ser Task<...>
            Assert.True(returnType.IsGenericType,
                "GetByDepartmentAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            // T interno da Task<T> deve ser IEnumerable<Process>
            var taskInner = returnType.GetGenericArguments().Single();

            Assert.True(taskInner.IsGenericType,
                "Task<T> interno deve ser genérico (IEnumerable<Process>).");

            Assert.Equal(typeof(IEnumerable<>), taskInner.GetGenericTypeDefinition());

            var enumerableInnerArg = taskInner.GetGenericArguments().Single();
            Assert.Equal(typeof(Process), enumerableInnerArg);
        }

        // -----------------------------------------------------------------------------
        // 4. Assinatura deve permanecer assíncrona/plural e sem overloads
        // -----------------------------------------------------------------------------

        [Fact]
        public void GetByDepartmentAsync_ShouldReturnCollection_NotSingleItem()
        {
            var method = typeof(IProcessRepository)
                .GetMethod("GetByDepartmentAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;

            // ainda é Task<...>
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByDepartmentAsync deve continuar retornando Task (assíncrono).");

            // Tipo dentro da Task<>
            var taskInner = returnType.GetGenericArguments().Single();

            // Task<IEnumerable<Process>> e não Task<Process>
            Assert.True(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner),
                "Retorno interno da Task deve continuar sendo uma coleção (IEnumerable<Process>).");

            Assert.NotEqual(typeof(Process), taskInner);
        }

        [Fact]
        public void IProcessRepository_ShouldNotIntroduceOverloads_For_GetByDepartmentAsync()
        {
            var overloads = typeof(IProcessRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByDepartmentAsync")
                .ToList();

            // Só pode existir um método com esse nome.
            // Se no futuro alguém adicionar algo tipo
            // GetByDepartmentAsync(long departmentId, bool onlyActive)
            // esse teste vai falhar e forçar atualização consciente.
            Assert.Single(overloads);
        }

        // -----------------------------------------------------------------------------
        // 5. Essa interface deve declarar exatamente 1 método extra além do IRepository
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessRepository_ShouldOnlyDeclare_GetByDepartmentAsync_AsExtraMethod()
        {
            var t = typeof(IProcessRepository);

            // Todos os métodos que a interface expõe (inclui herdados de IRepository<>)
            var allMethods = t.GetMethods();

            // Todos os métodos herdados de IRepository<Process> (e outras interfaces base, se houver no futuro)
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Agora filtramos só os métodos que pertencem especificamente a IProcessRepository,
            // ou seja, que não existem nas interfaces pai.
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos que haja exatamente UM método próprio:
            // Task<IEnumerable<Process>> GetByDepartmentAsync(long departmentId)
            Assert.Single(declaredHere);

            var only = declaredHere[0];
            Assert.Equal("GetByDepartmentAsync", only.Name);

            // Confirma assinatura de novo pra congelar contrato
            var p = only.GetParameters();
            Assert.Single(p);
            Assert.Equal("departmentId", p[0].Name);
            Assert.Equal(typeof(long), p[0].ParameterType);

            var returnType = only.ReturnType;
            Assert.True(returnType.IsGenericType);
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            var inner = returnType.GetGenericArguments().Single();
            Assert.True(inner.IsGenericType);
            Assert.Equal(typeof(IEnumerable<>), inner.GetGenericTypeDefinition());
            Assert.Equal(typeof(Process), inner.GetGenericArguments().Single());
        }

        // Helper para comparar parâmetros de método (mesmo nome e tipo)
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

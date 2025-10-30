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
    public class IProcessExecutionRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessExecutionRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IProcessExecutionRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IProcessExecutionRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IProcessExecutionRepository deve continuar pública.");

            Assert.False(t.IsSealed,
                "Se virou classe sealed, isso quebrou o contrato e deve ser revisado.");
            Assert.False(t.IsAbstract && t.IsClass,
                "Não pode virar classe abstrata sem alinhar o contrato.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<ProcessExecution>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessExecutionRepository_MustInherit_IRepository_Of_ProcessExecution()
        {
            var t = typeof(IProcessExecutionRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<ProcessExecution>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(ProcessExecution)
            );

            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(ProcessExecution), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Método GetByProcessAsync(processId)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessExecutionRepository_MustDeclare_GetByProcessAsync_WithExpectedSignature()
        {
            var t = typeof(IProcessExecutionRepository);

            var method = t.GetMethod("GetByProcessAsync",
                                     BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);
            Assert.Equal("GetByProcessAsync", method!.Name);

            // --- Parâmetros ---
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var param = parameters[0];

            // O parâmetro tem que continuar se chamando "processId"
            Assert.Equal("processId", param.Name);

            // E tem que continuar sendo long
            Assert.Equal(typeof(long), param.ParameterType);

            // --- Retorno ---
            // Esperado: Task<IEnumerable<ProcessExecution>>
            var returnType = method.ReturnType;

            // Retorno externo deve ser Task<...>
            Assert.True(returnType.IsGenericType,
                "Retorno deve ser Task<...>.");
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            // Pega o T interno da Task<T>
            var taskInner = returnType.GetGenericArguments().Single();

            // T interno deve ser IEnumerable<ProcessExecution>
            Assert.True(taskInner.IsGenericType,
                "Task<T> interno deve ser um tipo genérico IEnumerable<ProcessExecution>.");

            Assert.Equal(typeof(IEnumerable<>), taskInner.GetGenericTypeDefinition());

            var enumerableInnerArg = taskInner.GetGenericArguments().Single();
            Assert.Equal(typeof(ProcessExecution), enumerableInnerArg);
        }

        // -----------------------------------------------------------------------------
        // 4. Garantir que o método é assíncrono/plural e sem overloads
        // -----------------------------------------------------------------------------

        [Fact]
        public void GetByProcessAsync_MustReturnCollection_NotSingleItem()
        {
            var method = typeof(IProcessExecutionRepository)
                .GetMethod("GetByProcessAsync",
                    BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByProcessAsync deve continuar retornando Task (assíncrono).");

            var taskInner = returnType.GetGenericArguments().Single();
            Assert.True(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner),
                "O tipo interno de Task<> deve continuar sendo uma coleção (IEnumerable<ProcessExecution>), não um único ProcessExecution.");

            Assert.NotEqual(typeof(ProcessExecution), taskInner);
        }

        [Fact]
        public void IProcessExecutionRepository_ShouldNotIntroduceOverloads_For_GetByProcessAsync()
        {
            var overloads = typeof(IProcessExecutionRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByProcessAsync")
                .ToList();

            // Só pode existir um GetByProcessAsync.
            // Se começar a ter overload (ex: com filtro de status), esse teste vai falhar
            // e forçar quem mexeu a atualizar conscientemente.
            Assert.Single(overloads);
        }

        // -----------------------------------------------------------------------------
        // 5. (Opcional) Nenhum outro método declarado além dele
        // -----------------------------------------------------------------------------
        // Isso documenta que hoje o contrato extra é só GetByProcessAsync.
        // Se você adicionar novos métodos depois (ex: GetActiveExecutionsAsync),
        // é só ajustar esse teste conscientemente.

        [Fact]
        public void IProcessExecutionRepository_ShouldOnlyDeclare_GetByProcessAsync_AsExtraMethod()
        {
            var t = typeof(IProcessExecutionRepository);

            var allMethods = t.GetMethods(); // inclui herdados
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos que são próprios dessa interface (não herdados de IRepository<>)
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Esperamos exatamente 1 método próprio hoje: GetByProcessAsync(long processId)
            Assert.Single(declaredHere);

            var only = declaredHere[0];
            Assert.Equal("GetByProcessAsync", only.Name);

            // Confirma a assinatura desse único método "extra"
            var p = only.GetParameters();
            Assert.Single(p);
            Assert.Equal("processId", p[0].Name);
            Assert.Equal(typeof(long), p[0].ParameterType);

            var returnType = only.ReturnType;
            Assert.True(returnType.IsGenericType);
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            var inner = returnType.GetGenericArguments().Single();
            Assert.True(inner.IsGenericType);
            Assert.Equal(typeof(IEnumerable<>), inner.GetGenericTypeDefinition());
            Assert.Equal(typeof(ProcessExecution), inner.GetGenericArguments().Single());
        }

        // helper para comparar parâmetros (nome e tipo)
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

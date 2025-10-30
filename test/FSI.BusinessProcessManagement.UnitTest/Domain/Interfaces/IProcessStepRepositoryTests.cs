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
    public class IProcessStepRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessStepRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IProcessStepRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IProcessStepRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IProcessStepRepository deve continuar pública.");

            // Protege contra mudança indevida pra classe concreta
            Assert.False(t.IsSealed, "Se virou classe sealed, o contrato mudou e precisa atualizar o teste.");
            Assert.False(t.IsAbstract && t.IsClass,
                "Não pode virar classe abstrata/concreta sem alinhar.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<ProcessStep>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessStepRepository_MustInherit_IRepository_Of_ProcessStep()
        {
            var t = typeof(IProcessStepRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<ProcessStep>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(ProcessStep)
            );

            // Validação mais detalhada:
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);

            // T do IRepository<T> deve ser exatamente ProcessStep
            Assert.Equal(typeof(ProcessStep), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Método GetByProcessIdAsync(processId)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessStepRepository_MustDeclare_GetByProcessIdAsync_WithExpectedSignature()
        {
            var t = typeof(IProcessStepRepository);

            var method = t.GetMethod(
                "GetByProcessIdAsync",
                BindingFlags.Public | BindingFlags.Instance
            );

            Assert.NotNull(method);
            Assert.Equal("GetByProcessIdAsync", method!.Name);

            // --- Parâmetros ---
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var param = parameters[0];

            // O parâmetro deve continuar se chamando "processId"
            Assert.Equal("processId", param.Name);

            // O parâmetro deve continuar sendo do tipo long
            Assert.Equal(typeof(long), param.ParameterType);

            // --- Tipo de retorno ---
            // Esperado: Task<IEnumerable<ProcessStep>>
            var returnType = method.ReturnType;

            Assert.True(returnType.IsGenericType,
                "GetByProcessIdAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            // Recupera T de Task<T>
            var taskInner = returnType.GetGenericArguments().Single();

            // T deve ser IEnumerable<ProcessStep>
            Assert.True(taskInner.IsGenericType,
                "Task<T> interno deve ser IEnumerable<ProcessStep>.");

            Assert.Equal(typeof(IEnumerable<>), taskInner.GetGenericTypeDefinition());

            var enumerableInnerArg = taskInner.GetGenericArguments().Single();
            Assert.Equal(typeof(ProcessStep), enumerableInnerArg);
        }

        // -----------------------------------------------------------------------------
        // 4. Assinatura deve continuar assíncrona/plural e sem overloads
        // -----------------------------------------------------------------------------

        [Fact]
        public void GetByProcessIdAsync_ShouldReturnCollection_NotSingleItem()
        {
            var method = typeof(IProcessStepRepository)
                .GetMethod("GetByProcessIdAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;

            // Ainda deve ser um Task (assíncrono)
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByProcessIdAsync deve continuar retornando Task (assíncrono).");

            // T interno da Task<T>
            var taskInner = returnType.GetGenericArguments().Single();

            // Task<IEnumerable<ProcessStep>>, não Task<ProcessStep>
            Assert.True(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner),
                "O retorno interno de Task<> deve continuar sendo coleção (IEnumerable<ProcessStep>), não um único ProcessStep.");

            Assert.NotEqual(typeof(ProcessStep), taskInner);
        }

        [Fact]
        public void IProcessStepRepository_ShouldNotIntroduceOverloads_For_GetByProcessIdAsync()
        {
            var overloads = typeof(IProcessStepRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByProcessIdAsync")
                .ToList();

            // Só pode existir UM método com esse nome.
            // Se alguém criar:
            // GetByProcessIdAsync(long processId, bool onlyActive)
            // este teste vai falhar e forçar atualização consciente.
            Assert.Single(overloads);
        }

        // -----------------------------------------------------------------------------
        // 5. Essa interface deve declarar exatamente 1 método extra além de IRepository
        // -----------------------------------------------------------------------------

        [Fact]
        public void IProcessStepRepository_ShouldOnlyDeclare_GetByProcessIdAsync_AsExtraMethod()
        {
            var t = typeof(IProcessStepRepository);

            // Todos os métodos visíveis nesta interface (inclui herdados)
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<ProcessStep> e de outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos que são específicos de IProcessStepRepository
            // (ou seja, não herdados)
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos exatamente UM método próprio:
            // Task<IEnumerable<ProcessStep>> GetByProcessIdAsync(long processId)
            Assert.Single(declaredHere);

            var only = declaredHere[0];
            Assert.Equal("GetByProcessIdAsync", only.Name);

            // Confirma de novo a assinatura congelada
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
            Assert.Equal(typeof(ProcessStep), inner.GetGenericArguments().Single());
        }

        // Helper: compara assinatura de parâmetros (mesmo tipo e mesmo nome)
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

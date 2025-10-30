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
    public class IAuditLogRepositoryTests
    {
        // -------------------------------------------------------------------------------------
        // 1. Estrutura geral da interface
        // -------------------------------------------------------------------------------------

        [Fact]
        public void IAuditLogRepository_MustExist_AndBeInterface_AndBePublic()
        {
            var t = typeof(IAuditLogRepository);

            Assert.NotNull(t);
            Assert.True(t.IsInterface, "IAuditLogRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IAuditLogRepository deve continuar pública.");
            Assert.False(t.IsSealed, "Interfaces não devem ser sealed; se virou classe, está errado.");
        }

        [Fact]
        public void IAuditLogRepository_MustInherit_IRepository_Of_AuditLog()
        {
            var t = typeof(IAuditLogRepository);

            // A interface deve herdar exatamente 1 interface pai, IRepository<AuditLog>.
            var interfaces = t.GetInterfaces();

            // Exemplo esperado: typeof(IRepository<AuditLog>)
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" && // nome genérico aberto
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(AuditLog)
            );

            // Garantir que o tipo base genérico é exatamente IRepository<T> com T = AuditLog.
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericDef = repoInterface!.GetGenericTypeDefinition();
            var genericArgs = repoInterface.GetGenericArguments();

            // IRepository<T> deve ter 1 argumento de tipo
            Assert.Equal(1, genericArgs.Length);

            // O T deve ser AuditLog
            Assert.Equal(typeof(AuditLog), genericArgs[0]);

            // Verifica o nome da interface pai (não conseguimos garantir namespace sem ter a referência,
            // mas garantimos que o nome é IRepository`1, que é o esperado no seu domínio).
            Assert.Equal("IRepository`1", genericDef.Name);
        }

        // -------------------------------------------------------------------------------------
        // 2. Método GetByUserAsync
        // -------------------------------------------------------------------------------------

        [Fact]
        public void IAuditLogRepository_MustContain_GetByUserAsync_WithExpectedSignature()
        {
            var t = typeof(IAuditLogRepository);

            // Procura o método pelo nome
            var method = t.GetMethod("GetByUserAsync",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            // Nome do método
            Assert.Equal("GetByUserAsync", method!.Name);

            // ------------------------
            // Parâmetros
            // ------------------------
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var param = parameters[0];

            // Nome do parâmetro precisa continuar 'userId'
            Assert.Equal("userId", param.Name);

            // Tipo do parâmetro precisa continuar long
            Assert.Equal(typeof(long), param.ParameterType);

            // ------------------------
            // Tipo de retorno
            // ------------------------
            // Esperado: Task<IEnumerable<AuditLog>>
            var returnType = method.ReturnType;
            Assert.Equal(typeof(Task<>), returnType.IsGenericType ? returnType.GetGenericTypeDefinition() : returnType);

            // Extrai o tipo interno de Task<...>
            var taskInnerType = returnType.GetGenericArguments().Single();
            Assert.True(taskInnerType.IsGenericType, "Task<T> interno deve ser genérico (IEnumerable<AuditLog>).");

            // TaskInnerType deve ser IEnumerable<AuditLog>
            Assert.Equal(typeof(IEnumerable<>), taskInnerType.GetGenericTypeDefinition());

            // IEnumerable<T> -> T deve ser AuditLog
            var enumerableInnerArg = taskInnerType.GetGenericArguments().Single();
            Assert.Equal(typeof(AuditLog), enumerableInnerArg);
        }

        // -------------------------------------------------------------------------------------
        // 3. Contrato assíncrono e pluralidade de retorno
        // -------------------------------------------------------------------------------------

        [Fact]
        public void GetByUserAsync_ShouldReturnMultipleAuditLogs_AsTaskEnumerable()
        {
            var method = typeof(IAuditLogRepository).GetMethod("GetByUserAsync",
                BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            // Confirma que retorna Task de algo enumerável
            var returnType = method!.ReturnType;

            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByUserAsync deve continuar sendo assíncrico (retornando Task).");

            var taskInnerType = returnType.GetGenericArguments().Single();
            Assert.True(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInnerType),
                "O tipo dentro de Task<> deve continuar sendo uma coleção (IEnumerable<AuditLog>).");

            // E garantir que não está retornando Task<AuditLog> unitário:
            Assert.NotEqual(typeof(AuditLog), taskInnerType);
        }

        // -------------------------------------------------------------------------------------
        // 4. Não deve haver overloads estranhos
        // -------------------------------------------------------------------------------------

        [Fact]
        public void IAuditLogRepository_ShouldNotIntroduceUnexpectedOverloads_For_GetByUserAsync()
        {
            var overloads = typeof(IAuditLogRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByUserAsync")
                .ToList();

            // Só pode existir 1 método GetByUserAsync na interface.
            // Se alguém adicionar uma sobrecarga, isso força discussão em PR.
            Assert.Single(overloads);
        }
    }
}

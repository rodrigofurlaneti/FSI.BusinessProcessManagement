using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IRoleScreenPermissionRepositoryTests
    {
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica da interface
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleScreenPermissionRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IRoleScreenPermissionRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IRoleScreenPermissionRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IRoleScreenPermissionRepository deve continuar pública.");

            // Protege contra mudança acidental pra classe concreta/abstract
            Assert.False(t.IsSealed,
                "Se IRoleScreenPermissionRepository virou classe sealed, o contrato mudou e precisa ser revisado.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IRoleScreenPermissionRepository não pode virar classe abstrata sem atualizar os testes.");
        }

        // -----------------------------------------------------------------------------
        // 2. Herança de IRepository<RoleScreenPermission>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleScreenPermissionRepository_MustInherit_IRepository_Of_RoleScreenPermission()
        {
            var t = typeof(IRoleScreenPermissionRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<RoleScreenPermission>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(RoleScreenPermission)
            );

            // Extra: conferir generic arg explicitamente
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(RoleScreenPermission), genericArgs[0]);
        }

        // -----------------------------------------------------------------------------
        // 3. Método GetByRoleAndScreenAsync(roleId, screenId)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleScreenPermissionRepository_MustDeclare_GetByRoleAndScreenAsync_WithExpectedSignature()
        {
            var t = typeof(IRoleScreenPermissionRepository);

            var method = t.GetMethod(
                "GetByRoleAndScreenAsync",
                BindingFlags.Public | BindingFlags.Instance
            );

            Assert.NotNull(method);
            Assert.Equal("GetByRoleAndScreenAsync", method!.Name);

            // --- Parâmetros ---
            var parameters = method.GetParameters();
            Assert.Equal(2, parameters.Length);

            // Primeiro parâmetro: long roleId
            var pRole = parameters[0];
            Assert.Equal("roleId", pRole.Name);
            Assert.Equal(typeof(long), pRole.ParameterType);

            // Segundo parâmetro: long screenId
            var pScreen = parameters[1];
            Assert.Equal("screenId", pScreen.Name);
            Assert.Equal(typeof(long), pScreen.ParameterType);

            // --- Tipo de retorno ---
            // Esperado: Task<RoleScreenPermission?>
            var returnType = method.ReturnType;

            Assert.True(returnType.IsGenericType,
                "GetByRoleAndScreenAsync deve retornar Task<...>.");

            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            var taskInner = returnType.GetGenericArguments().Single();

            // O tipo interno da Task<T> deve ser RoleScreenPermission (nullable reference type continua sendo RoleScreenPermission em runtime)
            Assert.Equal(typeof(RoleScreenPermission), taskInner);
        }

        // -----------------------------------------------------------------------------
        // 4. Assinatura deve continuar assíncrona e sem overloads
        // -----------------------------------------------------------------------------

        [Fact]
        public void GetByRoleAndScreenAsync_ShouldReturnSingleEntity_NotCollection()
        {
            var method = typeof(IRoleScreenPermissionRepository)
                .GetMethod("GetByRoleAndScreenAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;

            // Deve continuar baseado em Task (assíncrono)
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByRoleAndScreenAsync deve continuar retornando Task (assíncrono).");

            // O tipo interno não deve ser coleção; deve ser uma única entidade (ou null)
            var taskInner = returnType.GetGenericArguments().Single();

            // Se alguém mudar retorno pra IEnumerable<RoleScreenPermission>, isso falha:
            Assert.False(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner) &&
                         taskInner != typeof(string),
                "O retorno interno de Task<> não deve ser coleção. Esperado apenas uma RoleScreenPermission (possível null).");

            Assert.Equal(typeof(RoleScreenPermission), taskInner);
        }

        [Fact]
        public void IRoleScreenPermissionRepository_ShouldNotIntroduceOverloads_For_GetByRoleAndScreenAsync()
        {
            var overloads = typeof(IRoleScreenPermissionRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByRoleAndScreenAsync")
                .ToList();

            // Só pode existir UM método com esse nome.
            // Se alguém criar, por exemplo:
            // GetByRoleAndScreenAsync(long roleId, long screenId, bool onlyActive)
            // -> esse teste vai acusar e vai forçar alteração consciente aqui.
            Assert.Single(overloads);
        }

        // -----------------------------------------------------------------------------
        // 5. Essa interface deve declarar só esse método especial além de IRepository<>
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRoleScreenPermissionRepository_ShouldOnlyDeclare_GetByRoleAndScreenAsync_AsExtraMethod()
        {
            var t = typeof(IRoleScreenPermissionRepository);

            // Todos os métodos visíveis, incluindo herdados
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<RoleScreenPermission> e outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos que são definidos especificamente em IRoleScreenPermissionRepository
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Hoje esperamos exatamente UM método próprio:
            // Task<RoleScreenPermission?> GetByRoleAndScreenAsync(long roleId, long screenId)
            Assert.Single(declaredHere);

            var only = declaredHere[0];
            Assert.Equal("GetByRoleAndScreenAsync", only.Name);

            // Reconfirma assinatura para travar contrato
            var p = only.GetParameters();
            Assert.Equal(2, p.Length);

            Assert.Equal("roleId", p[0].Name);
            Assert.Equal(typeof(long), p[0].ParameterType);

            Assert.Equal("screenId", p[1].Name);
            Assert.Equal(typeof(long), p[1].ParameterType);

            var returnType = only.ReturnType;
            Assert.True(returnType.IsGenericType);
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            var inner = returnType.GetGenericArguments().Single();
            Assert.Equal(typeof(RoleScreenPermission), inner);
        }

        // Helper para comparar parâmetros de método (mesmo tipo e mesmo nome, na mesma ordem)
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

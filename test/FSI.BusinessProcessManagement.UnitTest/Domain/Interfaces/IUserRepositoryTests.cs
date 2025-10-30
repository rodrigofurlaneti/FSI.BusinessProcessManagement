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
    public class IUserRepositoryTests
    {
        // 1. Estrutura básica da interface
        [Fact]
        public void IUserRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IUserRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IUserRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IUserRepository deve continuar pública.");

            // proteção contra virar classe concreta ou abstract
            Assert.False(t.IsSealed,
                "Se IUserRepository virou classe sealed, isso quebrou o contrato e precisa ser revisado.");
            Assert.False(t.IsAbstract && t.IsClass,
                "IUserRepository não pode virar classe abstrata/concreta sem alinhar arquitetura.");
        }

        // 2. Herança de IRepository<User>
        [Fact]
        public void IUserRepository_MustInherit_IRepository_Of_User()
        {
            var t = typeof(IUserRepository);

            var interfaces = t.GetInterfaces();

            // Deve herdar IRepository<User>
            Assert.Contains(interfaces, i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1" &&
                i.GetGenericArguments().Length == 1 &&
                i.GetGenericArguments()[0] == typeof(User)
            );

            // Verificação mais detalhada
            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition().Name == "IRepository`1"
            );

            Assert.NotNull(repoInterface);

            var genericArgs = repoInterface!.GetGenericArguments();
            Assert.Single(genericArgs);
            Assert.Equal(typeof(User), genericArgs[0]);
        }

        // 3. Método GetByUsernameAsync(string username)
        [Fact]
        public void IUserRepository_MustDeclare_GetByUsernameAsync_WithExpectedSignature()
        {
            var t = typeof(IUserRepository);

            var method = t.GetMethod(
                "GetByUsernameAsync",
                BindingFlags.Public | BindingFlags.Instance
            );

            Assert.NotNull(method);
            Assert.Equal("GetByUsernameAsync", method!.Name);

            // Parâmetros
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("username", p.Name);
            Assert.Equal(typeof(string), p.ParameterType);

            // Retorno esperado: Task<User?>
            var returnType = method.ReturnType;

            Assert.True(returnType.IsGenericType,
                "GetByUsernameAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            var taskInner = returnType.GetGenericArguments().Single();

            // Deve ser User (nullable reference type vira typeof(User) em runtime)
            Assert.Equal(typeof(User), taskInner);
        }

        [Fact]
        public void GetByUsernameAsync_MustReturnSingleEntity_NotCollection()
        {
            var method = typeof(IUserRepository)
                .GetMethod("GetByUsernameAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;

            // precisa ser Task-based (assíncrono)
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetByUsernameAsync deve continuar sendo assíncrono (Task<...>).");

            var taskInner = returnType.GetGenericArguments().Single();

            // NÃO pode virar Task<IEnumerable<User>>
            Assert.False(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner) &&
                         taskInner != typeof(string),
                "GetByUsernameAsync deve retornar uma única entidade User (ou null), não uma coleção.");

            Assert.Equal(typeof(User), taskInner);
        }

        [Fact]
        public void IUserRepository_ShouldNotIntroduceOverloads_For_GetByUsernameAsync()
        {
            var overloads = typeof(IUserRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetByUsernameAsync")
                .ToList();

            // Só pode haver 1 assinatura.
            // Se alguém criar GetByUsernameAsync(string username, bool includeInactive), isso vai falhar.
            Assert.Single(overloads);
        }

        // 4. Método GetRoleNamesAsync(long userId)
        [Fact]
        public void IUserRepository_MustDeclare_GetRoleNamesAsync_WithExpectedSignature()
        {
            var t = typeof(IUserRepository);

            var method = t.GetMethod(
                "GetRoleNamesAsync",
                BindingFlags.Public | BindingFlags.Instance
            );

            Assert.NotNull(method);
            Assert.Equal("GetRoleNamesAsync", method!.Name);

            // Parâmetros
            var parameters = method.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("userId", p.Name);
            Assert.Equal(typeof(long), p.ParameterType);

            // Retorno esperado: Task<IReadOnlyList<string>>
            var returnType = method.ReturnType;

            Assert.True(returnType.IsGenericType,
                "GetRoleNamesAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), returnType.GetGenericTypeDefinition());

            // Task<T> -> T deve ser IReadOnlyList<string>
            var taskInner = returnType.GetGenericArguments().Single();

            Assert.True(taskInner.IsGenericType,
                "O tipo interno de Task<T> deve ser genérico (IReadOnlyList<string>).");

            Assert.Equal(typeof(IReadOnlyList<>), taskInner.GetGenericTypeDefinition());

            var innerGeneric = taskInner.GetGenericArguments().Single();
            Assert.Equal(typeof(string), innerGeneric);
        }

        [Fact]
        public void GetRoleNamesAsync_ShouldReturnCollectionOfStrings_NotSingleString()
        {
            var method = typeof(IUserRepository)
                .GetMethod("GetRoleNamesAsync", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(method);

            var returnType = method!.ReturnType;

            // Task-based
            Assert.True(typeof(Task).IsAssignableFrom(returnType),
                "GetRoleNamesAsync deve continuar retornando Task<...>.");

            var taskInner = returnType.GetGenericArguments().Single();

            // Task<IReadOnlyList<string>> -> o inner deve implementar IEnumerable
            Assert.True(typeof(System.Collections.IEnumerable).IsAssignableFrom(taskInner),
                "O retorno interno de Task<> deve ser uma coleção (IReadOnlyList<string>).");

            // NÃO pode ser Task<string>
            Assert.NotEqual(typeof(string), taskInner);
        }

        [Fact]
        public void IUserRepository_ShouldNotIntroduceOverloads_For_GetRoleNamesAsync()
        {
            var overloads = typeof(IUserRepository)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.Name == "GetRoleNamesAsync")
                .ToList();

            // Só pode haver 1 assinatura.
            // Se alguém criar GetRoleNamesAsync(long userId, bool activeOnly) -> quebra.
            Assert.Single(overloads);
        }

        // 5. A interface deve declarar EXATAMENTE esses dois métodos além do IRepository<User>
        [Fact]
        public void IUserRepository_ShouldOnlyDeclare_ExpectedExtraMethods()
        {
            var t = typeof(IUserRepository);

            // Todos os métodos visíveis em IUserRepository (inclui herdados de IRepository<User>)
            var allMethods = t.GetMethods();

            // Métodos herdados de IRepository<User> e de outras interfaces base
            var parentMethods = t.GetInterfaces()
                                 .SelectMany(i => i.GetMethods())
                                 .Distinct()
                                 .ToList();

            // Métodos exclusivos de IUserRepository
            var declaredHere = allMethods
                .Where(m => !parentMethods.Any(pm =>
                    pm.Name == m.Name &&
                    pm.ReturnType == m.ReturnType &&
                    ParametersMatch(pm.GetParameters(), m.GetParameters())
                ))
                .ToList();

            // Esperamos exatamente DOIS métodos próprios:
            //   Task<User?> GetByUsernameAsync(string username);
            //   Task<IReadOnlyList<string>> GetRoleNamesAsync(long userId);
            Assert.Equal(2, declaredHere.Count);

            Assert.Contains(declaredHere, m => m.Name == "GetByUsernameAsync");
            Assert.Contains(declaredHere, m => m.Name == "GetRoleNamesAsync");
        }

        // Helper: compara parâmetros por tipo e nome (na ordem)
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

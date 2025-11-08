using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IRepositoryTests
    {
        [Fact]
        public void IRepository_MustBe_Public_Generic_Interface_WithOneParameter()
        {
            var t = typeof(IRepository<>);

            Assert.True(t.IsInterface, "IRepository<T> deve ser interface.");
            Assert.True(t.IsPublic, "IRepository<T> deve ser pública.");
            Assert.True(t.IsGenericTypeDefinition, "IRepository<T> deve ser definição genérica.");
            Assert.Equal(1, t.GetGenericArguments().Length);
        }

        [Fact]
        public void IRepository_Methods_Must_Have_Expected_Signatures()
        {
            var repoType = typeof(IRepository<>);
            var T = repoType.GetGenericArguments().Single();

            Method(repoType, "GetAllAsync",
                returns: typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(T)),
                parameters: Type.EmptyTypes);

            Method(repoType, "GetByIdAsync",
                returns: typeof(Task<>).MakeGenericType(T),
                parameters: new[] { typeof(long) });

            Method(repoType, "InsertAsync",
                returns: typeof(Task),
                parameters: new[] { T });

            Method(repoType, "UpdateAsync",
                returns: typeof(Task),
                parameters: new[] { T });

            Method(repoType, "DeleteAsync",
                returns: typeof(Task),
                parameters: new[] { typeof(long) });
        }

        private static void Method(Type repoType, string name, Type returns, Type[] parameters)
        {
            var m = repoType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                            .FirstOrDefault(x =>
                                x.Name == name &&
                                ReturnMatches(x.ReturnType, returns) &&
                                ParamsMatch(x.GetParameters(), parameters));

            Assert.NotNull(m);
        }

        private static bool ParamsMatch(ParameterInfo[] provided, Type[] expected)
        {
            if (provided.Length != expected.Length) return false;
            for (int i = 0; i < expected.Length; i++)
            {
                if (provided[i].ParameterType != expected[i]) return false;
            }
            return true;
        }

        private static bool ReturnMatches(Type provided, Type expected)
        {
            if (provided == expected) return true;

            // Lida com genéricos (Task<T>, IEnumerable<T>, etc.)
            if (provided.IsGenericType && expected.IsGenericType)
            {
                if (provided.GetGenericTypeDefinition() != expected.GetGenericTypeDefinition())
                    return false;

                var pArgs = provided.GetGenericArguments();
                var eArgs = expected.GetGenericArguments();
                if (pArgs.Length != eArgs.Length) return false;

                for (int i = 0; i < pArgs.Length; i++)
                {
                    if (pArgs[i] != eArgs[i]) return false;
                }
                return true;
            }

            return false;
        }
    }
}

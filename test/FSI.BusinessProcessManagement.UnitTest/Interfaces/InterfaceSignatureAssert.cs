using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Interfaces
{
    internal static class InterfaceSignatureAssert
    {
        public static void IsInterfaceWithNameAndNamespace(Type type, string expectedName, string expectedNamespace)
        {
            Assert.NotNull(type);
            Assert.True(type.IsInterface, $"O tipo {type} deve ser interface.");
            Assert.Equal(expectedName, type.Name);
            Assert.Equal(expectedNamespace, type.Namespace);
        }

        public static void InheritsGenericInterface(Type type, Type openGenericInterface, Type expectedGenericArgument)
        {
            Assert.True(openGenericInterface.IsInterface && openGenericInterface.IsGenericTypeDefinition,
                "openGenericInterface deve ser uma interface open-genérica (ex.: typeof(IGenericAppService<>)).");

            var found = false;
            foreach (var itf in type.GetInterfaces())
            {
                if (itf.IsGenericType && itf.GetGenericTypeDefinition() == openGenericInterface)
                {
                    var args = itf.GetGenericArguments();
                    Assert.Single(args);
                    Assert.Equal(expectedGenericArgument, args[0]);
                    found = true;
                    break;
                }
            }
            Assert.True(found, $"{type.Name} deve herdar {openGenericInterface.Name}<{expectedGenericArgument.Name}>.");
        }

        public static void DeclaresNoAdditionalMembers(Type type)
        {
            var declaredMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var declaredProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var declaredEvents = type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            Assert.Empty(declaredMethods);
            Assert.Empty(declaredProps);
            Assert.Empty(declaredEvents);
        }

        public static void GenericParameterHasClassConstraint(Type openGenericInterface)
        {
            Assert.True(openGenericInterface.IsGenericTypeDefinition);
            var gp = openGenericInterface.GetGenericArguments()[0];
            var attrs = gp.GenericParameterAttributes;

            var hasClassConstraint = (attrs & GenericParameterAttributes.ReferenceTypeConstraint) != 0;
            Assert.True(hasClassConstraint, $"{openGenericInterface.Name} deve ter constraint 'where TDto : class'.");
        }

        public static void HasCrudMethodsWithCorrectSignatures(Type closedGenericInterface, Type dtoType)
        {
            var map = new Dictionary<string, (Type ReturnType, Type[] Params)>
            {
                ["GetAllAsync"] = (typeof(Task<>).MakeGenericType(typeof(IEnumerable<>).MakeGenericType(dtoType)), Type.EmptyTypes),
                ["GetByIdAsync"] = (typeof(Task<>).MakeGenericType(dtoType), new[] { typeof(long) }),
                ["InsertAsync"] = (typeof(Task<>).MakeGenericType(typeof(long)), new[] { dtoType }),
                ["UpdateAsync"] = (typeof(Task), new[] { dtoType }),
                ["DeleteAsync"] = (typeof(Task), new[] { typeof(long) }),
            };

            foreach (var kv in map)
            {
                var name = kv.Key;
                var (retType, paramTypes) = kv.Value;

                var m = closedGenericInterface.GetMethod(name);
                Assert.NotNull(m);

                // retorno
                Assert.Equal(retType, m!.ReturnType);

                // parâmetros
                var pars = m.GetParameters();
                Assert.Equal(paramTypes.Length, pars.Length);
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    Assert.Equal(paramTypes[i], pars[i].ParameterType);
                }
            }
        }
    }
}

using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Entities;
using FSI.BusinessProcessManagement.Domain.Interfaces;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Interfaces
{
    public class IDepartmentRepositoryTests
    {
        [Fact]
        public void IDepartmentRepository_MustExist_AndBePublicInterface()
        {
            var t = typeof(IDepartmentRepository);

            Assert.NotNull(t);

            Assert.True(t.IsInterface, "IDepartmentRepository deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IDepartmentRepository deve ser pública.");
            Assert.False(t.IsClass, "IDepartmentRepository não pode ser classe.");
            Assert.False(t.IsSealed, "Se virou classe sealed, isso quebrou o contrato.");
        }

        [Fact]
        public void IDepartmentRepository_MustInherit_IRepository_Of_Department()
        {
            var t = typeof(IDepartmentRepository);
            var interfaces = t.GetInterfaces();

            var repoInterface = interfaces.FirstOrDefault(i =>
                i.IsGenericType &&
                i.GetGenericTypeDefinition() == typeof(IRepository<>));

            Assert.NotNull(repoInterface);

            var args = repoInterface!.GetGenericArguments();
            Assert.Single(args);
            Assert.Equal(typeof(Department), args[0]);
        }

        [Fact]
        public void IDepartmentRepository_ShouldNotDeclareExtraMethods_Yet()
        {
            var t = typeof(IDepartmentRepository);

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

            Assert.Empty(declaredHere);
        }

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

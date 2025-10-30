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
        // -----------------------------------------------------------------------------
        // 1. Estrutura básica genérica
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRepository_MustExist_AsPublicInterface_WithSingleGenericParameter()
        {
            var t = typeof(IRepository<>);

            Assert.NotNull(t);
            Assert.True(t.IsInterface, "IRepository<T> deve continuar sendo interface.");
            Assert.True(t.IsPublic, "IRepository<T> deve continuar pública.");

            // Deve ser genérica aberta com 1 parâmetro de tipo
            Assert.True(t.IsGenericTypeDefinition, "IRepository<T> deve ser genérica aberta.");
            Assert.Equal(1, t.GetGenericArguments().Length);

            var genericArg = t.GetGenericArguments()[0];
            Assert.Equal("TEntity", genericArg.Name);

            // Verifica o constraint "where TEntity : class"
            var constraints = genericArg.GetGenericParameterConstraints();
            // Para "class" constraint, não vem em constraints, vem em GenericParameterAttributes
            var attrs = genericArg.GenericParameterAttributes;
            Assert.True(attrs.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint),
                "IRepository<T> deve manter o constraint 'where TEntity : class'.");
        }

        // -----------------------------------------------------------------------------
        // 2. Deve declarar exatamente os 5 métodos esperados (sem extras)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRepository_ShouldDeclare_ExactlyFiveExpectedMethods_AndNoMore()
        {
            var t = typeof(IRepository<>);

            // todos os métodos públicos da interface
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // Lista esperada por nome
            var expectedNames = new[]
            {
                "GetAllAsync",
                "GetByIdAsync",
                "InsertAsync",
                "UpdateAsync",
                "DeleteAsync"
            };

            // Confere que temos exatamente 5
            Assert.Equal(5, methods.Length);

            // Confere que são exatamente esses nomes
            Assert.Equal(
                expectedNames.OrderBy(n => n),
                methods.Select(m => m.Name).OrderBy(n => n)
            );

            // Confere que cada nome é único
            Assert.Equal(methods.Length, methods.Select(m => m.Name).Distinct().Count());
        }

        // -----------------------------------------------------------------------------
        // 3. Verificação detalhada de cada método
        // -----------------------------------------------------------------------------

        [Fact]
        public void GetAllAsync_MustReturn_Task_Of_IEnumerable_TEntity_AndHaveNoParameters()
        {
            var t = typeof(IRepository<>);
            var method = t.GetMethod("GetAllAsync");

            Assert.NotNull(method);

            // Sem parâmetros
            Assert.Empty(method!.GetParameters());

            // Retorno: Task<IEnumerable<TEntity>>
            Assert.True(method.ReturnType.IsGenericType,
                "GetAllAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), method.ReturnType.GetGenericTypeDefinition());

            var taskInner = method.ReturnType.GetGenericArguments().Single();

            Assert.True(taskInner.IsGenericType,
                "Task<T> interno deve ser IEnumerable<TEntity>.");
            Assert.Equal(typeof(IEnumerable<>), taskInner.GetGenericTypeDefinition());

            var enumerableInnerArg = taskInner.GetGenericArguments().Single();
            Assert.Equal(
                typeof(IRepository<>).GetGenericArguments()[0],
                enumerableInnerArg
            );
        }

        [Fact]
        public void GetByIdAsync_MustReturn_Task_Of_TEntityNullable_AndTake_Long_Id()
        {
            var t = typeof(IRepository<>);
            var method = t.GetMethod("GetByIdAsync");

            Assert.NotNull(method);

            // Parâmetros
            var parameters = method!.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("id", p.Name);
            Assert.Equal(typeof(long), p.ParameterType);

            // Retorno: Task<TEntity?>
            Assert.True(method.ReturnType.IsGenericType,
                "GetByIdAsync deve retornar Task<...>.");
            Assert.Equal(typeof(Task<>), method.ReturnType.GetGenericTypeDefinition());

            var taskInner = method.ReturnType.GetGenericArguments().Single();
            // taskInner deve ser TEntity ou TEntity?
            // Em runtime, typeof(TEntity) e typeof(TEntity?) para ref types são o mesmo typeof(TEntity)
            Assert.Equal(
                typeof(IRepository<>).GetGenericArguments()[0],
                taskInner
            );
        }

        [Fact]
        public void InsertAsync_MustReturn_Task_AndTake_TEntity_Entity()
        {
            var t = typeof(IRepository<>);
            var method = t.GetMethod("InsertAsync");

            Assert.NotNull(method);

            var parameters = method!.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("entity", p.Name);

            // parâmetro deve ser do tipo TEntity
            var tEntity = typeof(IRepository<>).GetGenericArguments()[0];
            Assert.Equal(tEntity, p.ParameterType);

            // Task (sem tipo)
            Assert.Equal(typeof(Task), method.ReturnType);
        }

        [Fact]
        public void UpdateAsync_MustReturn_Task_AndTake_TEntity_Entity()
        {
            var t = typeof(IRepository<>);
            var method = t.GetMethod("UpdateAsync");

            Assert.NotNull(method);

            var parameters = method!.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("entity", p.Name);

            var tEntity = typeof(IRepository<>).GetGenericArguments()[0];
            Assert.Equal(tEntity, p.ParameterType);

            Assert.Equal(typeof(Task), method.ReturnType);
        }

        [Fact]
        public void DeleteAsync_MustReturn_Task_AndTake_Long_Id()
        {
            var t = typeof(IRepository<>);
            var method = t.GetMethod("DeleteAsync");

            Assert.NotNull(method);

            var parameters = method!.GetParameters();
            Assert.Single(parameters);

            var p = parameters[0];
            Assert.Equal("id", p.Name);
            Assert.Equal(typeof(long), p.ParameterType);

            Assert.Equal(typeof(Task), method.ReturnType);
        }

        // -----------------------------------------------------------------------------
        // 4. Nenhum método deve ter overloads
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRepository_Methods_ShouldNotHave_Overloads()
        {
            var t = typeof(IRepository<>);
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            // Para cada nome, só pode existir 1 método.
            var duplicateNames = methods
                .GroupBy(m => m.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            Assert.Empty(duplicateNames);
        }

        // -----------------------------------------------------------------------------
        // 5. Métodos devem continuar assíncronos (Task-based)
        // -----------------------------------------------------------------------------

        [Fact]
        public void IRepository_Methods_MustAllReturn_TaskOrTaskOfT()
        {
            var t = typeof(IRepository<>);
            var methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);

            foreach (var m in methods)
            {
                var rt = m.ReturnType;

                var isTask =
                    rt == typeof(Task) ||
                    (rt.IsGenericType && rt.GetGenericTypeDefinition() == typeof(Task<>));

                Assert.True(isTask,
                    $"O método {m.Name} deve continuar retornando Task ou Task<T>, mantendo o padrão assíncrono.");
            }
        }
    }
}

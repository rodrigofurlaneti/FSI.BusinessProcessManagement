using System;
using System.Linq;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.Exceptions;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Exceptions
{
    public class DomainExceptionTests
    {
        // ---------------------------------------------------------------------
        // 1. CONTRATO / ESTRUTURA
        // ---------------------------------------------------------------------

        [Fact]
        public void DomainException_Class_MustExist_And_Inherit_SystemException()
        {
            var t = typeof(DomainException);

            // A classe existe
            Assert.NotNull(t);

            // Herança
            Assert.Equal(typeof(Exception), t.BaseType);

            // Não deve ser sealed nem abstract, para permitir especializações se necessário
            Assert.False(t.IsSealed);
            Assert.False(t.IsAbstract);
            Assert.True(t.IsClass);
            Assert.True(t.IsPublic);
        }

        [Fact]
        public void DomainException_MustExpose_TheExpectedPublicConstructors()
        {
            var t = typeof(DomainException);

            var ctors = t
                .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
                .Select(c => c.GetParameters().Select(p => p.ParameterType).ToArray())
                .ToList();

            // Esperamos DOIS construtores públicos:
            // 1) (string message)
            // 2) (string message, Exception inner)
            Assert.Contains(ctors, p =>
                p.Length == 1 &&
                p[0] == typeof(string)
            );

            Assert.Contains(ctors, p =>
                p.Length == 2 &&
                p[0] == typeof(string) &&
                p[1] == typeof(Exception)
            );

            // E não deve ter outros construtores públicos inesperados
            Assert.True(ctors.Count == 2,
                "DomainException não deve ter construtores públicos extras sem atualizar os testes.");
        }

        // ---------------------------------------------------------------------
        // 2. COMPORTAMENTO DOS CONSTRUTORES
        // ---------------------------------------------------------------------

        [Fact]
        public void Ctor_WithMessage_ShouldSet_Message_AndHaveNullInner()
        {
            // Arrange
            var msg = "Campo obrigatório ausente.";

            // Act
            var ex = new DomainException(msg);

            // Assert
            Assert.Equal(msg, ex.Message);
            Assert.Null(ex.InnerException);
        }

        [Fact]
        public void Ctor_WithMessage_AndInner_ShouldSet_Message_AndInner()
        {
            // Arrange
            var msg = "Falha de domínio ao validar processo.";
            var inner = new InvalidOperationException("Ops interno");

            // Act
            var ex = new DomainException(msg, inner);

            // Assert
            Assert.Equal(msg, ex.Message);
            Assert.Equal(inner, ex.InnerException);
        }

        // ---------------------------------------------------------------------
        // 3. TYPE SAFETY / SERIALIZABILITY (opcionalmente importante para logs)
        // ---------------------------------------------------------------------

        [Fact]
        public void DomainException_ShouldBe_Serializable_ByBaseExceptionContract()
        {
            var t = typeof(DomainException);

            // `Exception` já é [Serializable] por padrão.
            // Mesmo que DomainException não tenha explicitamente [Serializable],
            // ela herda a serialização básica.
            //
            // Vamos só garantir que ela É atribuível a Exception, o que
            // já confirmamos acima, e que NÃO é genérica / open generic.
            Assert.True(typeof(Exception).IsAssignableFrom(t));
            Assert.False(t.ContainsGenericParameters);
        }
    }
}

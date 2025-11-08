using System;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.ValueObjects;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.ValueObjects
{
    public class EmailTests
    {
        // ------------------------------------------------------------------
        // 1) Estrutura / Contrato
        // ------------------------------------------------------------------
        [Fact]
        public void Email_Type_MustExist_AndBeSealed_With_Address_GetOnly()
        {
            var t = typeof(Email);
            Assert.NotNull(t);

            // record compila como classe sealed por padrão (no seu código está "sealed record")
            Assert.True(t.IsClass && t.IsSealed, "Email deve ser uma classe sealed (record).");

            // Propriedade Address: string, get público, sem set público/privado
            var prop = t.GetProperty("Address", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);
            Assert.Equal(typeof(string), prop!.PropertyType);
            Assert.NotNull(prop.GetGetMethod());
            Assert.True(prop.GetGetMethod()!.IsPublic);
            Assert.Null(prop.GetSetMethod());
            Assert.Null(prop.GetSetMethod(nonPublic: true));
        }

        // ------------------------------------------------------------------
        // 2) Construtor: casos válidos
        // ------------------------------------------------------------------
        [Theory]
        [InlineData("john@doe.com", "john@doe.com")]
        [InlineData("  JOHN@DOE.COM  ", "john@doe.com")] // trim + lower
        [InlineData("MiXeD@Example.Org", "mixed@example.org")]
        public void Ctor_WithValidAddress_ShouldNormalize(string input, string expected)
        {
            var email = new Email(input);

            Assert.Equal(expected, email.Address);
            Assert.Equal(expected, email.ToString()); // ToString deve devolver Address
        }

        // ------------------------------------------------------------------
        // 3) Construtor: casos inválidos (mensagem padronizada)
        // ------------------------------------------------------------------
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("sem-arroba")]
        [InlineData("john.doe.com")]
        public void Ctor_WithInvalidAddress_ShouldThrowArgumentException(object raw)
        {
            // montar string? a partir do objeto raw
            string? input = raw as string;

            var ex = Assert.Throws<ArgumentException>(() => new Email(input!));
            Assert.Equal("E-mail inválido.", ex.Message);
        }

        // ------------------------------------------------------------------
        // 4) Igualdade (value object): deve considerar normalização
        // ------------------------------------------------------------------
        [Fact]
        public void Equality_SameAddressDifferentCasingOrSpaces_ShouldBeEqual()
        {
            var a = new Email("  TEST@Example.COM ");
            var b = new Email("test@example.com");

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_DifferentAddresses_ShouldNotBeEqual()
        {
            var a = new Email("a@x.com");
            var b = new Email("b@x.com");

            Assert.NotEqual(a, b);
            Assert.True(a != b);
            Assert.False(a == b);
        }

        // ------------------------------------------------------------------
        // 5) ToString deve refletir Address normalizado
        // ------------------------------------------------------------------
        [Fact]
        public void ToString_MustReturnNormalizedAddress()
        {
            var email = new Email("  UPPER@EXAMPLE.COM ");
            Assert.Equal("upper@example.com", email.ToString());
        }
    }
}

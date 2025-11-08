using System;
using System.Reflection;
using FSI.BusinessProcessManagement.Domain.ValueObjects;
using Xunit;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.ValueObjects
{
    public class PasswordHashTests
    {
        // ------------------------------------------------------------------
        // 1) Estrutura / Contrato
        // ------------------------------------------------------------------
        [Fact]
        public void PasswordHash_Type_MustBeSealedRecord_With_Hash_GetOnly()
        {
            var t = typeof(PasswordHash);
            Assert.NotNull(t);
            Assert.True(t.IsClass && t.IsSealed, "PasswordHash deve ser um record sealed.");

            var prop = t.GetProperty("Hash", BindingFlags.Public | BindingFlags.Instance);
            Assert.NotNull(prop);
            Assert.Equal(typeof(string), prop!.PropertyType);
            Assert.NotNull(prop.GetGetMethod());
            Assert.True(prop.GetGetMethod()!.IsPublic);
            Assert.Null(prop.GetSetMethod());
            Assert.Null(prop.GetSetMethod(nonPublic: true));
        }

        // ------------------------------------------------------------------
        // 2) Construtor válido
        // ------------------------------------------------------------------
        [Theory]
        [InlineData("1234567890abcdef")]
        [InlineData("HASH_123")]
        public void Ctor_ValidHash_ShouldStoreValue(string value)
        {
            var ph = new PasswordHash(value);

            Assert.Equal(value, ph.Hash);
            Assert.Equal(value, ph.ToString());
        }

        // ------------------------------------------------------------------
        // 3) Construtor inválido (nulo, vazio, espaço)
        // ------------------------------------------------------------------
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_NullOrEmpty_ShouldThrowArgumentException(object? raw)
        {
            string? value = raw as string;
            var ex = Assert.Throws<ArgumentException>(() => new PasswordHash(value!));
            Assert.Equal("Password hash is required.", ex.Message);
        }

        // ------------------------------------------------------------------
        // 4) Construtor inválido (muito longo)
        // ------------------------------------------------------------------
        [Fact]
        public void Ctor_TooLong_ShouldThrowArgumentException()
        {
            var tooLong = new string('x', 256);

            var ex = Assert.Throws<ArgumentException>(() => new PasswordHash(tooLong));
            Assert.Equal("Password hash too long.", ex.Message);
        }

        // ------------------------------------------------------------------
        // 5) Igualdade (records com mesmo hash devem ser iguais)
        // ------------------------------------------------------------------
        [Fact]
        public void Equality_SameHash_ShouldBeEqual()
        {
            var a = new PasswordHash("hashABC");
            var b = new PasswordHash("hashABC");

            Assert.Equal(a, b);
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        [Fact]
        public void Equality_DifferentHash_ShouldNotBeEqual()
        {
            var a = new PasswordHash("hash1");
            var b = new PasswordHash("hash2");

            Assert.NotEqual(a, b);
            Assert.True(a != b);
            Assert.False(a == b);
        }

        // ------------------------------------------------------------------
        // 6) ToString deve retornar o hash
        // ------------------------------------------------------------------
        [Fact]
        public void ToString_MustReturnHash()
        {
            var ph = new PasswordHash("abcXYZ");
            Assert.Equal("abcXYZ", ph.ToString());
        }
    }
}

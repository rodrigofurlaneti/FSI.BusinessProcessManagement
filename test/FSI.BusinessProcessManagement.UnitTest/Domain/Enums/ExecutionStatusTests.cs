using FSI.BusinessProcessManagement.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSI.BusinessProcessManagement.UnitTests.Domain.Enums
{
    public class ExecutionStatusTests
    {
        [Fact]
        public void Enum_ShouldExist_WithExpectedUnderlyingType()
        {
            var enumType = typeof(ExecutionStatus);

            Assert.True(enumType.IsEnum, "ExecutionStatus deve continuar sendo um enum.");
            Assert.Equal(typeof(int), Enum.GetUnderlyingType(enumType));
        }

        [Fact]
        public void Enum_ShouldContain_ExpectedMembers_InOrder_AndValues()
        {
            var names = Enum.GetNames(typeof(ExecutionStatus));
            var values = Enum.GetValues(typeof(ExecutionStatus)).Cast<int>().ToArray();

            // Verifica a ordem nominal e semântica
            var expectedNames = new[] { "Pendente", "Iniciado", "Concluido", "Cancelado" };
            var expectedValues = new[] { 0, 1, 2, 3 };

            Assert.Equal(expectedNames, names);
            Assert.Equal(expectedValues, values);
        }

        [Theory]
        [InlineData(ExecutionStatus.Pendente, 0)]
        [InlineData(ExecutionStatus.Iniciado, 1)]
        [InlineData(ExecutionStatus.Concluido, 2)]
        [InlineData(ExecutionStatus.Cancelado, 3)]
        public void Enum_Values_ShouldMatch_ExpectedIntegers(ExecutionStatus status, int expectedValue)
        {
            Assert.Equal(expectedValue, (int)status);
        }

        [Fact]
        public void ToString_ShouldReturn_ExpectedNames()
        {
            Assert.Equal("Pendente", ExecutionStatus.Pendente.ToString());
            Assert.Equal("Iniciado", ExecutionStatus.Iniciado.ToString());
            Assert.Equal("Concluido", ExecutionStatus.Concluido.ToString());
            Assert.Equal("Cancelado", ExecutionStatus.Cancelado.ToString());
        }

        [Fact]
        public void Enum_ShouldHave_NoAdditionalMembers()
        {
            var names = Enum.GetNames(typeof(ExecutionStatus));
            Assert.Equal(4, names.Length);
        }
    }
}

using GreenEnergyHub.TimeSeries.Benchmark;
using Xunit;

namespace GreenEnergyHub.TimeSeries.Tests.Core.EnumValueIsDefined
{
    public abstract class EnumIsDefinedTests
    {
        private readonly IEnumValueIsDefined _logic;

        public EnumIsDefinedTests(IEnumValueIsDefined logic)
        {
            _logic = logic;
        }

        private enum Kæledyr
        {
            Hund = 1,
            Kat = 2,
            Fisk = 3,
            Surdej = 10,
        }

        [Fact]
        public void KnownEnumType()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)Kæledyr.Hund);
            Assert.True(actual);
        }

        [Fact]
        public void KnownEnumTypeFromCast()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)(Kæledyr)3);
            Assert.True(actual);
        }

        [Fact]
        public void UnknownEnumTypeFromCast()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)Kæledyr.Surdej);
            Assert.False(actual);
        }
    }
}

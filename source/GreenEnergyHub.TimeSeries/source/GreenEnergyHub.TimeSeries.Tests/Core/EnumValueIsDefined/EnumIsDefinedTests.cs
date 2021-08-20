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

        private enum LocalPets
        {
            LC_Dog = 1,
            LC_Cat = 2,
            LC_Fish = 3,
            LC_Sourdough = 10,
        }

        [Fact]
        public void KnownEnumType()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)LocalPets.LC_Dog);
            Assert.True(actual);
        }

        [Fact]
        public void KnownEnumTypeFromCast()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)(LocalPets)3);
            Assert.True(actual);
        }

        [Fact]
        public void UnknownEnumTypeFromCast()
        {
            var actual = _logic.CheckValueIsDefined<Pets>((int)LocalPets.LC_Sourdough);
            Assert.False(actual);
        }
    }
}

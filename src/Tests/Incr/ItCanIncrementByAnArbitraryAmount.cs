using MickeySmith;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Tests.Incr
{
    public class ItCanIncrementByAnArbitraryAmount
    {
        private readonly string _key = "ItCanIncrementByAnArbitraryAmount";
        private readonly Session _session = SessionFactory.GetSession();
        private int _currentValue = 433;

        [Fact]
        public void GivenAnExistingValueWhenIncrementingBy1000ThenTheValueIsCorrect()
        {
            this
                .Given(x => x.GivenAnExistingValue())
                .When(x => x.WhenIncrementingBy1000())
                .Then(x => x.ThenTheValueIsCorrect())
                .BDDfy();
        }

        private void GivenAnExistingValue()
        {
            _session.Set(_key, _currentValue);
        }

        private void WhenIncrementingBy1000()
        {
            _session.Incr(_key, 1000);
            _currentValue += 1000;
        }

        private void ThenTheValueIsCorrect()
        {
            ((int) _session.Get(_key)).ShouldBe(_currentValue);
        }

        ~ItCanIncrementByAnArbitraryAmount()
        {
            _session.Delete(_key);
        }
    }
}
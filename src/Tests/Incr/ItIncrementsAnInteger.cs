using System.Diagnostics;
using MickeySmith;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Tests.Incr
{
    public class ItIncrementsAnInteger
    {
        private readonly Session _session = SessionFactory.GetSession();
        private string _key = "ItIncrementsAnInteger";
        private int _currentValue = 5;

        [Fact]
        public void GivenAnExistingValueWhenIncrementingTheValueTheValueIsIncremented()
        {
            Debug.Listeners.Add(new DefaultTraceListener());
            this
                .Given(x => x.GivenAnExistingIntegerValue())
                .When(x => x.WhenIncrementingTheValue())
                .Then(x => x.ThenTheValueIsIncremented())
                .BDDfy();
        }

        void GivenAnExistingIntegerValue()
        {
            _session.Set(_key, _currentValue);
        }

        void WhenIncrementingTheValue()
        {
            _session.Incr(_key);
            _currentValue++;
        }

        void ThenTheValueIsIncremented()
        {
            ((int)_session.Get(_key)).ShouldBe(_currentValue);
        }

        ~ItIncrementsAnInteger()
        {
            _session.Delete(_key);
        }
    }
}

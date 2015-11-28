using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MickeySmith;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Tests.Incr
{
    public class ItCanIncrementOverMultipleThreadsWithConsistency
    {
        private readonly string _key = "ItCanIncrementOverMultipleThreadsWithConsistency";
        private readonly Session _session = SessionFactory.GetSession();
        private int _currentValue = 433;

        [Fact]
        public void GivenAnExistingValueWhenIncrementingIn5WorkersThenTheValueIsCorrect()
        {
            this
                .Given(x => x.GivenAnExistingValue())
                .When(x => x.WhenIncrementingIn5Workers())
                .Then(x => x.ThenTheValueIsCorrect())
                .BDDfy();
        }

        private void GivenAnExistingValue()
        {
            _session.Set(_key, _currentValue);
        }

        private void WhenIncrementingIn5Workers()
        {
            var tasks = Enumerable.Range(0, 5)
                .Select(_ => Task.Run(() =>
                {
                    _session.Incr(_key);
                }))
                .ToArray();
            Task.WaitAll(tasks);

            _currentValue += 5;
        }

        private void ThenTheValueIsCorrect()
        {
            ((int)_session.Get(_key)).ShouldBe(_currentValue);
        }

        ~ItCanIncrementOverMultipleThreadsWithConsistency()
        {
            _session.Delete(_key);
        }
    }
}
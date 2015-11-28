using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MickeySmith;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Tests.Incr
{
    public class ItThrowsAnExceptionIfTheValueIsNotAnInteger
    {
        private readonly Session _session = SessionFactory.GetSession();
        private string _key = "ItThrowsAnExceptionIfTheValueIsNotAnInteger";

        [Fact]
        public void GivenAnExistingNonIntegerValueWhenIncrementingTheValueItThrowsAnException()
        {
            this
                .Given(x => x.GivenAnExistingNonIntegerValue())
                .Then(x => x.ThenItThrowsWhenIncrementingTheValue())
                .BDDfy();
        }

        void GivenAnExistingNonIntegerValue()
        {
            _session.Set(_key, "value");
        }

        void ThenItThrowsWhenIncrementingTheValue()
        {
            var aggregateException = Should.Throw<AggregateException>(() => _session.Incr(_key));
            var exception =  aggregateException.InnerExceptions.First();

            exception.ShouldBeOfType<InvalidOperationException>();
            exception.Message.ShouldBe("Only integer values can be incremented or decremented");
        }

        ~ItThrowsAnExceptionIfTheValueIsNotAnInteger()
        {
            _session.Delete(_key);
        }
    }
}

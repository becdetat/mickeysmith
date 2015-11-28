using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MickeySmith;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Tests.Delete
{
    public class ItDeletesAKey
    {
        private readonly Session _session = SessionFactory.GetSession();
        private const string _key = "ItDeletesAKey";

        [Fact]
        public void GivenAPresentKeyValueWhenDeletingTheKeyTheValueIsNull()
        {
            this
                .Given(x => x.GivenAPresentKey())
                .When(x => x.WhenDeletingTheKey())
                .Then(x => x.ThenTheValueIsNull())
                .BDDfy();
        }

        void GivenAPresentKey()
        {
            _session.Set(_key, "value");
        }

        void WhenDeletingTheKey()
        {
            _session.Delete(_key);
        }

        void ThenTheValueIsNull()
        {
            ((object)_session.Get(_key)).ShouldBe(null);
        }
    }
}

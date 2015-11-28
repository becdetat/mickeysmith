using MickeySmith;

namespace Tests
{
    public static class SessionFactory
    {
        public static Session GetSession()
        {
            return new Session(MickeySmithTestbed.Program.ConnectionString);
        }
    }
}
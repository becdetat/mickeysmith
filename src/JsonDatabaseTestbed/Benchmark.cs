using System;

namespace MickeySmithTestbed
{
    public class Benchmark : IDisposable
    {
        private readonly DateTime _start = DateTime.Now;

        public void Dispose()
        {
            var elapsedMs = (DateTime.Now - _start).TotalMilliseconds;

            Cmd.WriteInfoLine($"Operation completed in {elapsedMs}ms");
        }
    }
}
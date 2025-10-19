using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace OrleansBasics
{
    public class HelloGrain : Orleans.Grain, IHello
    {
        private readonly ILogger _logger;

        public HelloGrain(ILogger<HelloGrain> logger)
        {
            _logger = logger;
        }

        Task<string> IHello.SayHello(string greeting)
        {
            _logger.LogInformation("SayHello message received: greeting = '{Greeting}'", greeting);
            return Task.FromResult($"\n Client said: '{greeting}', so HelloGrain says: Hello!");
        }
    }
}
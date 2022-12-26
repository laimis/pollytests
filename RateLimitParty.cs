// create retry policy

using Polly;
using Polly.RateLimit;

internal class RateLimitParty
{
    private static Policy _rateLimit = Policy.RateLimit(10, TimeSpan.FromSeconds(1), maxBurst: 10);

    private static Policy _retryPolicy = 
        Policy.Handle<RateLimitRejectedException>().RetryForever(onRetry: ex => 
        {
            var rateLimitRejectedException = ex as RateLimitRejectedException;
            var sleepDuration = rateLimitRejectedException?.RetryAfter ?? TimeSpan.FromSeconds(1);

            System.Console.WriteLine($"Retrying after sleeping for {sleepDuration}");
            Thread.Sleep(sleepDuration);
        });

    private static Policy _wrapped = Policy.Wrap(_retryPolicy, _rateLimit);

    internal static void RunAsync()
    {
        try
        {
            for (int i = 0; i < 100; i++)
            {
                var number = i;

                _rateLimit.Execute(() =>
                {
                    System.Console.WriteLine($"Starting {number}");
                });
            }
        }
        catch(RateLimitRejectedException ex)
        {
            System.Console.WriteLine($"Rate limit exceeded: {ex.Message}");
        }

        Console.WriteLine("This time, we will add forever retry, press enter to continue...");
        Console.ReadLine();

        // now let's try with retry
        for (int i = 0; i < 100; i++)
        {
            var number = i;

            _retryPolicy.Execute(() =>
            {
                _rateLimit.Execute(() =>
                {
                    System.Console.WriteLine($"Starting {number}");
                });
            });
        }

        Console.WriteLine("Lastly, the same thing, but using wrapped policy, press enter to continue...");
        Console.ReadLine();

        for (int i = 0; i < 100; i++)
        {
            var number = i;

            _wrapped.Execute(() =>
            {
                System.Console.WriteLine($"Starting {number}");
            });
        }
    }
}
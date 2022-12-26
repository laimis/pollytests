using Polly;

namespace pollytests
{
    public class RetryParty
    {
        internal static void Run()
        {
            var source = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var waitAndRetryPolicy = Policy.Handle<Exception>()
                .WaitAndRetry(
                    retryCount: 3,
                    sleepDurationProvider: retryAttempt => {
                        var seconds = Random.Shared.Next(retryAttempt, retryAttempt * 2);
                        // var seconds = Math.Pow(2, retryAttempt); // exponential back off OMG
                        var duration = TimeSpan.FromSeconds(seconds);
                        Console.WriteLine($"Waiting {duration} before next retry.");
                        return duration;
                    }
                );

            var retryPolicy = Policy.Handle<Exception>()
                .Retry(3, (exception, retryCount) => {
                    Console.WriteLine($"Retry {retryCount} due to: {exception}.");
                });

            void Retry(Policy retryPolicy)
            {
                retryPolicy.Execute(action: () => {
                        Console.WriteLine("Hello World! Simple");
                        throw new Exception("Something went wrong");
                    }
                );
            }

            PolicyResult RetryCapture(Policy retryPolicy) => retryPolicy.ExecuteAndCapture(() => {
                        Console.WriteLine("Hello World! Capture");
                        throw new Exception("Something went wrong with capture");
                    }
                );

            PolicyResult<int> RetryWithReturn(Policy retryPolicy) => retryPolicy.ExecuteAndCapture(() => {
                        Console.WriteLine("Hello World! Capture with return");
                        return 1;
                    }
                );

            void RetryWithOutsideCancellationToken(Policy retryPolicy, CancellationToken token)
            {
                retryPolicy.Execute(action: tkn => {
                        Console.WriteLine("Hello World!");
                        Console.WriteLine("Token: " + tkn);
                        Console.WriteLine(" equal?: " + (tkn == token));
                        throw new Exception("Something went wrong");
                    },
                    cancellationToken: token
                );
            }

            // Retry(retryPolicy);
            // RetryWithOutsideCancellationToken(retryPolicy, source.Token);
            var captured = RetryCapture(waitAndRetryPolicy);
            var result = captured switch {
                { Outcome: OutcomeType.Successful } => "Success",
                { Outcome: OutcomeType.Failure } => "Failure",
                _ => "Unknown"
            };
            Console.WriteLine(result);

            var capturedWithReturn = RetryWithReturn(waitAndRetryPolicy);
            Console.WriteLine(capturedWithReturn);


            // now just retry policy without any wait
            RetryCapture(retryPolicy);
        }
    }
}
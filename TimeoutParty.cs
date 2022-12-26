using Polly;
using Polly.Timeout;

namespace pollytests
{
    public class TimeoutParty
    {
        internal async static Task AsyncRun()
        {
            var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(2));

            await timeout.ExecuteAsync(async () => {
                Console.WriteLine("Hello World, about to sleep for 5 seconds");
                await Task.Delay(5000);
                Console.WriteLine("Hello World, I'm awake from optimistic timeout");
            });

            try
            {
                await timeout.ExecuteAsync(async ct => {
                    Console.WriteLine("Hello World, about to sleep for 5 seconds with ct");
                    await Task.Delay(5000, ct);
                    Console.WriteLine("Hello World, I'm awake from optimistic timeout with ct");
                }, CancellationToken.None);

                // NOTE how you never see "I'm awake from optimistic timeout with ct" because
                // the timeout is hit and the task gets cancelled
            }
            catch(TimeoutRejectedException e)
            {
                Console.WriteLine("Optimistic timeout rejected exception was thrown " + e.Message);
            }

            var captured = await timeout.ExecuteAndCaptureAsync(async ct => {
                Console.WriteLine("Hello World, about to sleep for 5 seconds, but no catch needed");
                await Task.Delay(5000, ct);
                Console.WriteLine("Hello World, I'm awake from optimistic timeout with capture");
            }, CancellationToken.None);

            Console.WriteLine("Outcome: " + captured.Outcome);
        }

        internal static void Run()
        {
            var timeout = Policy.Timeout(TimeSpan.FromSeconds(2));

            timeout.Execute(() => {
                Console.WriteLine("Hello World, about to sleep for 5 seconds");
                System.Threading.Thread.Sleep(5000);
                Console.WriteLine("Hello World, I'm awake from optimistic timeout");
            });

            var pessimisticTimeout = Policy.Timeout(TimeSpan.FromSeconds(2), TimeoutStrategy.Pessimistic);

            try
            {
                pessimisticTimeout.Execute(() => {
                    Console.WriteLine("Hello World, about to sleep for 5 seconds");
                    System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("Hello World, I'm awake from pessimistic timeout");
                });
            }
            catch(TimeoutRejectedException e)
            {
                Console.WriteLine("Pessimistic timeout rejected exception was thrown " + e.Message);
            }

            var pessimisticWithOnTimeout = Policy.Timeout(
                TimeSpan.FromSeconds(2),
                TimeoutStrategy.Pessimistic,
                (context, timespan, task) => {
                    Console.WriteLine("OnTimeout called");
                }
            );

            try
            {
                pessimisticWithOnTimeout.Execute(() => {
                    Console.WriteLine("Hello World, about to sleep for 5 seconds");
                    System.Threading.Thread.Sleep(5000);
                    Console.WriteLine("Hello World, I'm awake from pessimistic timeout with on timeout handler");
                });
            }
            catch(TimeoutRejectedException e)
            {
                Console.WriteLine("Pessimistic w/ on timeout timeout rejected exception was thrown " + e.Message);
            }
        }

        
    }
}
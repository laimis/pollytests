using Polly;

namespace pollytests
{
    public class BulkheadParty
    {
        internal static void RunAsync()
        {
            var policy = Policy.BulkheadAsync(
                maxParallelization: 2,
                maxQueuingActions: 2,
                onBulkheadRejectedAsync: (context) =>
                {
                    System.Console.WriteLine("Rejected " + context.Count);
                    return Task.CompletedTask;
                });

            for (int i = 0; i < 10; i++)
            {
                var number = i;
                policy.ExecuteAsync(async () =>
                {
                    System.Console.WriteLine($"Starting {number}");
                    await Task.Delay(1000);
                    System.Console.WriteLine($"Ending {number}");
                });
            }
        }
    }
}
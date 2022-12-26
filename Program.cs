// create retry policy

using pollytests;

var partyMode = 3;

switch (partyMode)
{
    case 0:
        RetryParty.Run();
        break;
        
    case 1:
        TimeoutParty.Run();

        System.Console.WriteLine("Press enter to continue...");
        System.Console.ReadLine();

        await TimeoutParty.AsyncRun();

        System.Console.WriteLine("Press enter to exit...");
        System.Console.ReadLine();
        break;
        
    case 2:
        BulkheadParty.RunAsync();
        System.Console.WriteLine("Press enter to exit after bulkhead party...");
        System.Console.ReadLine();
        break;

    case 3:
        RateLimitParty.RunAsync();
        System.Console.WriteLine("Press enter to exit after rate limit party...");
        System.Console.ReadLine();
        break;
    default:
        break;
}
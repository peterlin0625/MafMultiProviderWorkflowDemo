using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.HumanInLoop;
public sealed class ConsoleUserConfirmationService
    : IUserConfirmationService
{
    public Task<bool> ConfirmAsync(
        string message,
        CancellationToken cancellationToken)
    {
        Console.WriteLine(message);
        Console.Write("Confirm? (y/n): ");

        var input = Console.ReadLine();
        return Task.FromResult(
            input?.Equals("y", StringComparison.OrdinalIgnoreCase) == true);
    }
}

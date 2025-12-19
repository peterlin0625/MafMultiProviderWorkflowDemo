using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.HumanInLoop;

public interface IUserConfirmationService
{
    Task<bool> ConfirmAsync(
        string message,
        CancellationToken cancellationToken);
}

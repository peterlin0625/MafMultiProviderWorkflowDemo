using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;
public interface IPolicyLlmAdvisor
{
    Task<PolicyLlmSuggestion?> SuggestAsync(
        string userInput,
        CancellationToken cancellationToken);
}
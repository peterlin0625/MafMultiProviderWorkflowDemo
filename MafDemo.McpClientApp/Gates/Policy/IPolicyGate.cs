using MafDemo.McpClientApp.Domain;
using MafDemo.McpClientApp.Domain.DomainGate;

namespace MafDemo.McpClientApp.Gates.Policy; 
public interface IPolicyGate
{
    PolicyDecision Evaluate(string userInput, DomainGateResult domainResult);
}

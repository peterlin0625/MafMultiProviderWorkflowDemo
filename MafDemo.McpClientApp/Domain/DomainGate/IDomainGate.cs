using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// Domain Gate：判斷使用者輸入是否屬於 CloudPrint 任務領域
/// </summary>
public interface IDomainGate
{
    DomainGateResult Evaluate(string userInput);
}

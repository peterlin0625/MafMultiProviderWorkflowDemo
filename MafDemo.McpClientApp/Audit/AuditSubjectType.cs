using System;
using System.Collections.Generic;
using System.Text;
 
namespace MafDemo.McpClientApp.Audit;

/// <summary>
/// 表示這筆 Audit 事件所記錄的主體是什麼
/// </summary>
public enum AuditSubjectType
{
    DomainGate,
    PolicyGate,
    Agent,
    Workflow,
    Proposal,
    Tool,
    System
}


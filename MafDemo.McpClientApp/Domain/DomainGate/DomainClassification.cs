using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Domain.DomainGate;

/// <summary>
/// Domain Gate 的唯一合法分類結果
/// </summary>
public enum DomainClassification
{
    /// <summary>
    /// 明確屬於 CloudPrint 任務領域
    /// </summary>
    InDomain,

    /// <summary>
    /// 非直接列印任務，但可引導成列印需求
    /// </summary>
    AdjacentDomain,

    /// <summary>
    /// 與 CloudPrint 完全無關，應直接拒絕
    /// </summary>
    OutOfDomain
}

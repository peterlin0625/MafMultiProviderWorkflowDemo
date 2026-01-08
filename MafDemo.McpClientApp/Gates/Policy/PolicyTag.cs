using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

/// <summary>
/// Phase 2 的白名單風險標籤（LLM 只能從這裡選）
/// </summary>
public enum PolicyTag
{
    political,
    adult,
    hate_or_violence,
    ip
}

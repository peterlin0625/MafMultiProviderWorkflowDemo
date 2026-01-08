using System;
using System.Collections.Generic;
using System.Text;

namespace MafDemo.McpClientApp.Gates.Policy;

public enum PolicyReasonCode
{
    ALLOWED = 0,

    // Blocked
    POLITICAL_CONTENT = 1001,
    ADULT_CONTENT = 1002,
    HATE_OR_VIOLENCE = 1003,

    // Requires human review
    IP_UNCERTAIN = 2001
}

using System;
using System.Collections.Generic;
using System.Text;

namespace CloudPrint.McpServer.Options;

public class StyleDataOptions
{
    public string BaseUrl { get; set; } = "";    // ex: http://localhost:5010
    public string AllStylesEndpoint { get; set; } = "/styles/all";
}

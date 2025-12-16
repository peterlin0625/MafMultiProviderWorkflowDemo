namespace CloudPrint.McpServer.Models;

public sealed class PrintGuide
{
    public string ProductCode { get; set; } = string.Empty;
    public string RecommendedPaper { get; set; } = string.Empty;
    public string RecommendedSize { get; set; } = string.Empty;
    public string ColorMode { get; set; } = "Color"; // Color / Mono
    public string MinResolutionHint { get; set; } = string.Empty;
    public string ExtraNotes { get; set; } = string.Empty;
}

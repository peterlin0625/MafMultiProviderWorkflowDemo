using ModelContextProtocol.Server;

namespace CloudPrint.McpServer.Tools;

[McpServerToolType]
public static class GetProductInfoTool
{
    public record Request(string ProductId);
    public record Response(string ProductId, string Name, string Description);

    [McpServerTool]
    public static Response GetProductInfo(Request request)
    {
        // 模擬資料
        return new Response(
            request.ProductId,
            Name: "明信片",
            Description: "ibon 雲端列印的熱門商品，可彩色列印、可客製文字"
        );
    }
}

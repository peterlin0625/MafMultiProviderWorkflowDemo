namespace CloudPrint.McpServer.Models;

public sealed class ProductInfo
{
    /// <summary>內部商品代碼，例如 P202500123</summary>
    public string ProductCode { get; set; } = string.Empty;

    /// <summary>顯示名稱（例如：婚禮感謝小卡）</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>簡短說明，給 AI 導購用的 Summary</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>商品類型，例如：Sticker / Postcard / Calendar / Label</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>網站前端的大致路徑提示</summary>
    public string PathHint { get; set; } = string.Empty;
}

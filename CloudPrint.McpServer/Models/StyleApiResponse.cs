namespace CloudPrint.McpServer.Models;

public class StyleGroupDto
{
    public long GroupId { get; set; }
    public string Name { get; set; } = "";
    public string ShowName { get; set; } = "";
    public string Sex { get; set; } = "";
    public List<StyleDto> Styles { get; set; } = new();
}

public class StyleDto
{
    public string StyleId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShowName { get; set; } = "";
    public string Thumb { get; set; } = "";
}

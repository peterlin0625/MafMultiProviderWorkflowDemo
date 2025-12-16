namespace CloudPrint.DataApi.Models;

public class StyleGroup
{
    public long GroupId { get; set; }
    public string Name { get; set; } = "";
    public string ShowName { get; set; } = "";
    public string Sex { get; set; } = "";
    public List<StyleItem> Styles { get; set; } = new();
}

public class StyleItem
{
    public string StyleId { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShowName { get; set; } = "";
    public string Thumb { get; set; } = "";
}

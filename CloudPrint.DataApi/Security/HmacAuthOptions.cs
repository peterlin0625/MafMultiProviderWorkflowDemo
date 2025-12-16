namespace CloudPrint.DataApi.Security;

public sealed class HmacAuthOptions
{
    public string ApiKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
}

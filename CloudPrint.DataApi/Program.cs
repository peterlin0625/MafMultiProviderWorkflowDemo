using CloudPrint.DataApi.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.Configure<HmacAuthOptions>(
    builder.Configuration.GetSection("Hmac"));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // XML Doc ¤ä´©
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);

    options.SwaggerDoc("v1", new()
    {
        Title = "CloudPrint DataApi",
        Version = "v1",
        Description = ""
    });
});


//builder.Services.AddTransient<HmacAuthMiddleware>();
builder.Services.AddTransient<HmacAuthReplayAttackMiddleware>();

builder.Services.AddControllers();

var app = builder.Build();

// HMAC Middleware
//app.UseMiddleware<HmacAuthMiddleware>();
app.UseMiddleware<HmacAuthReplayAttackMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MafDemo AI Workflow API v1");
    c.DocumentTitle = "DataApi API";
});


app.MapControllers();

app.Run();

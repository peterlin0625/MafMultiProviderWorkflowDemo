using CloudPrint.DataApi.Security;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMemoryCache();
builder.Services.Configure<HmacAuthOptions>(
    builder.Configuration.GetSection("Hmac"));

//builder.Services.AddTransient<HmacAuthMiddleware>();
builder.Services.AddTransient<HmacAuthReplayAttackMiddleware>();

builder.Services.AddControllers();

var app = builder.Build();

// HMAC Middleware
app.UseMiddleware<HmacAuthMiddleware>();




app.MapControllers();

app.Run();

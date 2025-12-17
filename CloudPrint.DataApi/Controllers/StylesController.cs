using CloudPrint.DataApi.Models;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CloudPrint.DataApi.Controllers;

[ApiController]
[Route("[controller]")]
public class StylesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    public StylesController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [HttpGet]
    [Route("All")]
    public async Task<IEnumerable<StyleGroup>> GetAllStyles()
    {
        var path = Path.Combine(_env.ContentRootPath, "mock-data.json");
        var json = await System.IO.File.ReadAllTextAsync(path);

        return JsonSerializer.Deserialize<List<StyleGroup>>(json) ?? new();
    }
}

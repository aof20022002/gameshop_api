using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet]
    public IEnumerable<string> Get()
    {
        return new string[] { "Keyboard", "Mouse", "Monitor" };
    }

    [HttpPost]
    public IActionResult Post([FromBody] string product)
    {
        return Ok($"Added product: {product}");
    }
}

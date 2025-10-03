using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    [HttpGet]
    public String Get()
    {
        return "User controller is working!";
    }
    [HttpGet("{id}")]
    public String Get(int id)
    {
        return $"User controller is working! ID: {id}";
    }
}

namespace nsPocOrlns.WebHost3.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{

    [HttpGet]
    public IActionResult Ping()
    {
        return Ok($"Hello :)");
    }
}

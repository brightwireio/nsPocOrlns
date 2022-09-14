namespace nsPocOrlns.WebHost3.Controllers;

[ApiController]
[Route("api/ping")]
public class PingController : ControllerBase
{

    [HttpGet]
    public IActionResult Ping()
    {

        var wpip = Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_IP");
        var wpports = Environment.GetEnvironmentVariable("WEBSITE_PRIVATE_PORTS");


        return Ok($"Hello from v0.029 :) {wpip} {wpports}");
    }
}

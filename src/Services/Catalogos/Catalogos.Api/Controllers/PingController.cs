using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers;

[ApiController]
[Route("catalogos")]
public class PingController : ControllerBase
{
	[HttpGet("ping")]
	public IActionResult Ping() => Ok(new { ok = true, service = "catalogos" });
}

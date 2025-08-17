// Catalogos.Api/Controllers/PingController.cs
using Microsoft.AspNetCore.Mvc;

namespace Catalogos.Api.Controllers;

[ApiController]
[Route("ping")]
public class PingController : ControllerBase
{
	[HttpGet] public IActionResult Get() => Ok(new { ok = true });
}

using Microsoft.AspNetCore.Mvc;

namespace CryptoOnRamp.API.Controllers;

// for webhook testing purposes

[Route("api/null")]
[ApiController]
public sealed class NullController : ControllerBase
{
    [HttpPost]
    public IActionResult Post()
    {
        return Ok();
    }
}

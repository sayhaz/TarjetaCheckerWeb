using Microsoft.AspNetCore.Mvc;

namespace TarjetaCheckerWebShutdown.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShutdownController : ControllerBase
    {
        private readonly IHostApplicationLifetime _appLifetime;

        public ShutdownController(IHostApplicationLifetime appLifetime)
        {
            _appLifetime = appLifetime;
        }

        [HttpPost]
        public IActionResult Stop()
        {
            _appLifetime.StopApplication();
            return Ok("Servidor detenido.");
        }
    }
}
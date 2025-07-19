
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TarjetaCheckerWeb; // 👈 Importa el namespace donde está StatusHub

namespace TarjetaCheckerWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CheckerController : ControllerBase
    {
        private readonly IHubContext<StatusHub> _hubContext;

        public CheckerController(IHubContext<StatusHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("CheckerController funcionando con SignalR StatusHub.");
        }
    }
}

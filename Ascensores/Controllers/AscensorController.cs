using Ascensores.Models;
using Ascensores.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ascensores.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AscensorController : ControllerBase
    {
        private readonly AscensorService _service;
        public AscensorController(AscensorService service)
        {
            _service = service;
        }


        [HttpGet]
        public ActionResult<Ascensor> GetEstado()
        {
            return Ok(_service.GetEstado());
        }


        [HttpPost("abrir")]
        public async Task<IActionResult> AbrirPuertas()
        {
            await _service.AbrirPuertasAsync();
            return Ok();
        }


        [HttpPost("cerrar")]
        public async Task<IActionResult> CerrarPuertas()
        {
            await _service.CerrarPuertasAsync();
            return Ok();
        }


        [HttpPost("iniciar")]
        public async Task<IActionResult> Iniciar()
        {
            await _service.IniciarAsync();
            return Ok();
        }


        [HttpPost("detener")]
        public async Task<IActionResult> Detener()
        {
            await _service.DetenerAsync();
            return Ok();
        }
    }
}

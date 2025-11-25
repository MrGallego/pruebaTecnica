using Ascensores.Models.DTO;
using Ascensores.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Ascensores.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SolicitudesController : ControllerBase
    {
        private readonly AscensorService _service;
        public SolicitudesController(AscensorService service)
        {
            _service = service;
        }


        [HttpGet]
        public async Task<IActionResult> GetPendientes()
        {
            var list = await _service.GetSolicitudesPendientes();
            return Ok(list);
        }


        [HttpPost]
        public async Task<IActionResult> CrearSolicitud([FromBody] DtoSolicitud dto)
        {
            if (dto.Piso < 1 || dto.Piso > 20) return BadRequest("Piso fuera de rango (1-20)");
            await _service.CrearSolicitud(dto.Piso);
            return Accepted();
        }
    }
}

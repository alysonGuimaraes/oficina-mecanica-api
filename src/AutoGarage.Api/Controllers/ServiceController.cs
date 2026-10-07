using Microsoft.AspNetCore.Mvc;

namespace AutoGarage.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ServiceController : ControllerBase
    {

        [HttpGet]
        public IActionResult GetAll()
        {
            var services = new[]
            {
            new { Id = 1, Nome = "Troca de Óleo", Preco = 150.00 },
            new { Id = 2, Nome = "Alinhamento e Balanceamento", Preco = 120.00 },
            new { Id = 3, Nome = "Troca de Pastilhas de Freio", Preco = 250.00 }
            };

            return Ok(services);
        }

        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var service = new { Id = id, Nome = "Diagnóstico Eletrônico", Preco = 200.00 };
            return Ok(service);
        }
    }
}

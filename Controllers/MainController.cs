using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pawlio.Models;
using Microsoft.AspNetCore.Diagnostics;

namespace Pawlio.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api")]
    public class MainController : Controller
    {
        private PostgreSqlDbContext _dbContext;

        public MainController(PostgreSqlDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet]
        public string Main() => "It's Works! V2";

        // 17.03.2024 �afak
        // Uygulama hatalar�n� response olarak d�nmesi i�in
        // Ayr�ca loglama eklenecek
        [Route("/error")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult HandleError()
        {
            var exceptionHandlerFeature = HttpContext.Features.Get<IExceptionHandlerFeature>()!;
            var message = exceptionHandlerFeature.Error.Message;
            if (message == "LOGOUT") return Unauthorized();
            return Problem(message, statusCode: 400);
        }

        //[HttpGet("cities")]
        //public List<City> Cities()
        //{
        //    return (this._dbContext.Cities.Include(c => c.Districties).ToList());
        //}

        [HttpGet("symptoms")]
        public List<Symptom> Symptoms()
        {
            return this._dbContext.Symptoms.ToList();
        }
    }
}
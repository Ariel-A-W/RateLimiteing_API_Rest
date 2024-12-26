using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;

namespace WebApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", 
            "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly List<WeatherForecast> datas; 

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;

            int i = 1;
            var lstWeath = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)],
                Id = i++
            }).ToArray();

            datas = new List<WeatherForecast>();
            datas.AddRange(lstWeath);
        }

        [HttpGet]
        [EnableRateLimiting("extremelyLimit")]
        public IEnumerable<WeatherForecast> GetList()
        {
            return datas.ToArray();
        }

        [HttpGet("/GetById")]
        [EnableRateLimiting("fixed")]
        public WeatherForecast GetById(int id)
        {
            var weath = datas.FirstOrDefault(x => x.Id == id)!;

            if (weath is null)
                return null!;

            return weath;
        }

        [HttpPost("/Add")]
        public int Post(
            [FromBody] WeatherForecast param
        )
        {
            return 1;
        }

        [HttpPut("/Update/:id")]
        public int Update(
            int id, [FromBody] WeatherForecast param
        )
        {
            return 1;
        }

        [HttpDelete("/Delete/:id")]
        public int Delete(
            int id
        )
        {
            return 1;
        }
    }
}

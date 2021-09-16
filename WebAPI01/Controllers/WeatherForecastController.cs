using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace WebAPI01.Controllers
{
    public class ParamModel
    {
        public int number { get; set; }
        public int multiplier { get; set; }
    }

    public class ParamModel2
    {
        public int number { get; set; }
    }


    [ApiController]
    [Route("api/v1/[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
         "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
      };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var rng = new Random();
            var returnResults = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return Ok(returnResults);
        }

        [HttpGet("getbynumber")]
        public IActionResult GetByNumber([FromQuery] int number)
        {
            if (number <= 0) return BadRequest();

            var rng = new Random();
            var returnResults = Enumerable.Range(1, number).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return Ok(returnResults);
        }

        [HttpGet("getbybody")]
        public IActionResult GetByBody([FromBody] ParamModel2 model)
        {
            if (model == null || model.number <= 0) return BadRequest();

            var rng = new Random();
            var returnResult = Enumerable.Range(1, model.number).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return Ok(returnResult);
        }

        [HttpGet("{number}/getbydifferentnumber")]
        public IActionResult GetByDifferentNumber(int number)
        {
            if (number <= 0) return BadRequest();

            var rng = new Random();
            var returnResults = Enumerable.Range(1, number).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();

            return Ok(returnResults);
        }

        [HttpPost("postbydifferentnumber")]
        public IActionResult PostByDifferentNumber([FromBody] ParamModel model)
        {
            if (model == null || model.number <= 0 || model.multiplier <= 0) return BadRequest("Invalid Param");

            var rng = new Random();

            var returnArray = Enumerable.Range(1, model.multiplier).Select(i =>
            {
                return Enumerable.Range(1, model.number).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
             .ToArray();
            }).ToArray();

            return Ok(new { numAllrecord = model.multiplier * model.number, returnArray });
        }

        [Route("{multiplier}/postbybodyandroute")]
        [HttpPost]
        public IActionResult PostByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            var rng = new Random();

            var returnArray = Enumerable.Range(1, multiplier).Select(i =>
            {
                return Enumerable.Range(1, model.number).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)]
                })
             .ToArray();
            }).ToArray();

            return Ok(new { numAllrecord = multiplier * model.number, returnArray });
        }

        [Route("{multiplier}/postbybodyandroute2")]
        [HttpPost]
        public IActionResult PostByBodyAndRoute2(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            var rng = new Random();

            var returnArray = Enumerable.Range(1, multiplier).Select(m =>
            {
                return Enumerable.Range(1, model.number).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(m),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)],
                    Collector = rng.Next(0,10) % 2 == 1 ? "Collector-1" : "Collector-2"
                })
                .ToArray();
            }).ToArray();

            var foldArray = returnArray.SelectMany(x => x).ToArray();
            var groupedArray = foldArray.GroupBy(f => new { date = $"{f.Date:yyyy-MM-dd}", f.Collector }, (k, g) => new WeatherForecast()
            {
                Date = DateTime.Parse(k.date),
                Collector = k.Collector,
                TemperatureC = g.Count() == 0 ? 0 : g.Sum(x => x.TemperatureC)/g.Count(),
                Summary = string.Join(',', g.Select(t => t.Summary).ToList())
            }).ToArray();

            return Ok(new { numAllrecord = multiplier * model.number, groupedArray });
        }

        [Route("{multiplier}/patchbybodyandroute")]
        [HttpPatch]
        public IActionResult PatchByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            return Ok(new { numAllrecord = multiplier * model.number, message = $"patch by {multiplier}:{model.number}" });
        }

        [Route("{multiplier}/deletebybodyandroute")]
        [HttpDelete]
        public IActionResult DeleteByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            return Ok(new { numAllrecord = multiplier * model.number, message = $"delete by {multiplier}:{model.number}" });
        }

        [Route("{multiplier}/optionsbybodyandroute")]
        [HttpOptions]
        public IActionResult OptionsByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            return Ok(new { numAllrecord = multiplier * model.number, message = $"options by {multiplier}:{model.number}" });
        }

        [Route("{multiplier}/putbybodyandroute")]
        [HttpPut]
        public IActionResult PutByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            return Ok(new { numAllrecord = multiplier * model.number, message = $"put by {multiplier}:{model.number}" });
        }

        [Route("{multiplier}/headbybodyandroute")]
        [HttpHead]
        public IActionResult HeadByBodyAndRoute(int multiplier, ParamModel2 model)
        {
            if (model == null || model.number <= 0 || multiplier <= 0) return BadRequest("Invalid Param");

            return Ok(new { numAllrecord = multiplier * model.number, message = $"head by {multiplier}:{model.number}" });
        }
    }
}

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.CircuitBreaker;

namespace CallerApi.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<ActionResult<List<WeatherForecast>>> Get()
    {
        try
        {
            HttpClient httpClient = _httpClientFactory.CreateClient("WeatherApi");
            List<WeatherForecast> weatherForecasts = await httpClient.GetFromJsonAsync<List<WeatherForecast>>("WeatherForecast");
            return weatherForecasts;
        }
        catch (BrokenCircuitException)
        {
            _logger.LogCritical("The weather service is now inoperative. Please try again later.");
        }
        catch (System.Exception)
        {
            throw;
        }

        return new List<WeatherForecast>();
    }
}

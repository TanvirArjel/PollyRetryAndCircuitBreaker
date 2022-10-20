using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using CallerApi.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly;
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

            Context pollyContext = new Polly.Context().WithLogger<WeatherForecastController>(_logger);


            HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, "WeatherForecast");
            requestMessage.SetPolicyExecutionContext(pollyContext);

            HttpResponseMessage httpResponseMessage = await httpClient.SendAsync(requestMessage);
            httpResponseMessage.EnsureSuccessStatusCode();

            List<WeatherForecast> weatherForecasts = await httpResponseMessage.Content.ReadFromJsonAsync<List<WeatherForecast>>();
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

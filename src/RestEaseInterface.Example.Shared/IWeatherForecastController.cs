using RestEase;

namespace RestEaseInterface.Example.Shared;

public interface IWeatherForecastController
{
    [Get("GetWeatherForecast")]
    Task<IEnumerable<WeatherForecast>> Get();

    [Get]
    string Test();
}
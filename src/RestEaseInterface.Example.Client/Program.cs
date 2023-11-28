using RestEase;
using RestEaseInterface.Example.Shared;

IWeatherForecastController api = RestClient.For<IWeatherForecastController>("http://localhost:5045");

WeatherForecast[] weatherInfos = api.Get().Result.ToArray();
foreach (var weatherInfo in weatherInfos) {
    Console.WriteLine($"Date: {weatherInfo.Date}. Summary: {weatherInfo.Summary}. Temp°C: {weatherInfo.TemperatureC}. Temp-Fahrenheit: {weatherInfo.TemperatureF}");
}

Console.ReadLine();
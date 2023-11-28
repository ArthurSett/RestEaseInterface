# [RestEase](https://github.com/canton7/RestEase) + [RestEaseInterface](https://github.com/ArthurSett/RestEaseInterface) = ![Heart](./imgs/heart.png)

RestEaseInterface is designed to enable fast, seamless, and strongly-typed communication between .NET applications using REST. It is built on top of [RestEase](https://github.com/canton7/RestEase), allowing you to create REST clients using a configured interface. RestEaseInterface complements this by generating corresponding endpoints with [minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) based on the same interface to handle incoming requests. Swagger supported.

## Implementation
Follow these steps to implement RestEaseInterface:

1. **Create an Interface**: Follow the [RestEase documentation](https://github.com/canton7/RestEase/blob/master/README.md) to design an interface. This interface should be accessible to both the client and server, as shown in the example project.

2. **Client Initialization**: On the client side, initialize the interface either [directly](https://github.com/canton7/RestEase#quick-start) or using [HttpClientFactory](https://github.com/canton7/RestEase#using-httpclientfactory).

3. **Server-Side Implementation**: On the server side (AspNetCore is required), create a class that implements the configured interface. This class will contain your business logic. *Note: Do not create multiple classes for a single configured RestEase interface.*

4. **Server-Side Configuration in Startup/Program.cs**:
   - In the `IServiceCollection`, use `.UseRestEaseApiInterface()`. This method searches for configured interfaces and classes at startup using reflection.
   - In the `IServiceProvider`, use `.AddRestEaseInterface()`. This dynamically constructs [minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) based on these interfaces.

## Example
Shared code - available on server and client
```csharp
using RestEase;

namespace RestEaseInterface.Example.Shared;

public interface IWeatherForecastController
{
    [Get("GetWeatherForecast")]
    IEnumerable<WeatherForecast> Get();
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
}
```

Server code - Program.cs

```csharp
using RestEaseInterface;
using RestEaseInterface.Swagger;

var builder = WebApplication.CreateBuilder(args);

// [...]

//Swagger implementation (optional)
builder.Services.AddSwaggerGen(x =>
{
    x.EnableAnnotations();
    x.DocumentFilter<RestEaseInterfaceSwaggerDocumentFilter>();
});

//RestEase implementation
builder.Services.AddRestEaseInterface(); 

var app = builder.Build();

// [...]

//RestEase implementation
app.UseRestEaseInterface(); 

```

Client code - Program.cs
```csharp
using RestEase;
using RestEaseInterface.Example.Shared;

IWeatherForecastController api = RestClient.For<IWeatherForecastController>("http://localhost:<port>");

WeatherForecast[] weatherInfos = api.Get().Result.ToArray();
foreach (var weatherInfo in weatherInfos) {
    Console.WriteLine($"Date: {weatherInfo.Date}. Summary: {weatherInfo.Summary}."
    + $"Temp°C: {weatherInfo.TemperatureC}. Temp-Fahrenheit: {weatherInfo.TemperatureF}");
}

Console.ReadLine();
```
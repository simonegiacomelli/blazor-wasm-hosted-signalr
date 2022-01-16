using BlazorWebAssemblySignalRApp.Shared;

namespace BlazorWebAssemblySignalRApp.Server.Services;

public class WeatherForecastService : IWeatherForecastService
{
    public string Echo(string name, int value)
    {
        return $"Echo: {name} {value}";
    }
}
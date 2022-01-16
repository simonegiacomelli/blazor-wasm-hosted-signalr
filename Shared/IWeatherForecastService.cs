namespace BlazorWebAssemblySignalRApp.Shared;

public interface IWeatherForecastService
{
    public string Echo(string name, int value);
}
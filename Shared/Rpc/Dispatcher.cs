namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class Dispatcher
{
    private Dictionary<String, Type> dict = new();

    public void Register<T>()
    {
        dict[GetNameFromType<T>()] = typeof(T);
    }

    public Type GetTypeFromName(string name)
    {
        return dict[name];
    }

    public static string GetNameFromType<T>() => typeof(T).FullName!;

    public static object? Dispatch(object provider, string methodName, string payload)
    {
        return "todo";
    }
}
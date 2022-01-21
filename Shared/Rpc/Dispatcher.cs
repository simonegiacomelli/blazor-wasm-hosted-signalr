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

    public static string Dispatch(object provider, string methodName, string payload)
    {
        var m = provider.GetType().GetMethod(methodName);
        var ms = new MethodSerializer(m!);
        var arg0 = ms.ArgsSerializer.Deserializer(payload);
        var args = (object[])arg0;
        var res = m.Invoke(provider, args);
        var tt = res.GetType();
        var prop = tt.GetProperty("Result")!;
        var unwrapped = prop.GetValue(res, null);
        var ser = ms.ReturnSerializer.Serializer(unwrapped);
        return ser;
    }
}
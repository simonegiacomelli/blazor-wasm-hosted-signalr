namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public delegate Task<String> Dispatcher(RpcInterfaceMessage msg);

public class Registry
{
    private Dictionary<String, Type> dict = new();

    public void Register<T>()
    {
        dict[GetNameFromType<T>()] = typeof(T);
    }

    public Type GetTypeFromName(string name)
    {
        var res = dict.GetValueOrDefault(name);
        if (res == null)
            throw new InvalidOperationException($"No interface registered under name `{name}`");
        return res;
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

    public Dispatcher Dispatcher(Func<Type, object> activator)
    {
        Dispatcher dispatcher = async (msg) =>
        {
            var type = GetTypeFromName(msg.TypeName);
            var o = activator(type);
            var res = Dispatch(o, msg.MethodName, msg.Payload);
            return res;
        };
        return dispatcher;
    }
}
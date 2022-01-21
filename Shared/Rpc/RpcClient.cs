using System.Reflection;
using BlazorWebAssemblySignalRApp.Shared.Rpc;

public class RpcClient : DispatchProxy
{
    public Func<string, string, string, Task<string>> dispatcher;
    private string typeName;

    public static T Create<T>(Func<string, string, string, Task<string>> dispatcher)
    {
        var proxy = Create<T, RpcClient>();
        var p = proxy as RpcClient;
        p.dispatcher = dispatcher;
        p.typeName = Dispatcher.GetNameFromType<T>();
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        var m = targetMethod!;
        var pi = m.GetParameters();
        var methodName = m.Name;

        var ser = new MethodSerializer(m);
        var argsSer = ser.ArgsSerializer.Serializer(args[0]);
        var tresult = dispatcher.Invoke(typeName, methodName, argsSer);
        var tresultResult = tresult.Result;
        var r = ser.ReturnSerializer.Deserializer(tresultResult);

        var methodInfo = typeof(Task).GetMethod("FromResult");
        var genericMethodInfo = methodInfo.MakeGenericMethod(ser.ReturnSerializer.Type);
        var result2 = genericMethodInfo.Invoke(null, new object[] { r });
        return result2;
    }
}
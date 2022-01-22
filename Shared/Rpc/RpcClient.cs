using System.Reflection;
using BlazorWebAssemblySignalRApp.Shared.Rpc;

public class RpcClient : DispatchProxyAsync
{
    public Dispatcher dispatcher;
    private string typeName;

    public static T Create<T>(Dispatcher dispatcher)
    {
        var proxy = Create<T, RpcClient>();
        var p = proxy as RpcClient;
        p.dispatcher = dispatcher;
        p.typeName = Registry.GetNameFromType<T>();
        return proxy;
    }

    public override object Invoke(MethodInfo method, object[] args)
    {
        throw new NotImplementedException();
    }

    public override Task InvokeAsync(MethodInfo method, object[] args)
    {
        throw new NotImplementedException();
    }

    public override async Task<T> InvokeAsyncT<T>(MethodInfo method, object[] args)
    {
        var m = method;
        var methodName = m.Name;

        var ser = new MethodSerializer(m);
        var argsSer = ser.ArgsSerializer.Serializer(args);
        var tresult = dispatcher.Invoke(new RpcInterfaceMessage(typeName, methodName, argsSer));
        var tresultResult = await tresult;
        var r = ser.ReturnSerializer.Deserializer(tresultResult);
        var result3 = (T)r;
        return result3;
    }
}
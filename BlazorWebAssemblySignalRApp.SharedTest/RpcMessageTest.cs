using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BlazorWebAssemblySignalRApp.SharedTest;

[TestFixture]
public class RpcMessageTest
{
    [Test]
    public void test1()
    {
        var msg = RpcMessage.Encode("foo", "bar");
        var restored = RpcMessage.Decoder(msg);
        Assert.AreEqual("foo", restored.Name);
        Assert.AreEqual("bar", restored.Payload);
    }
}

[TestFixture]
public class RpcSingleInstanceExchangeTest
{
    class SumRequest : IRequest<SumResponse>
    {
        public int a { get; set; }
        public int b { get; set; }
    }

    class SumResponse
    {
        public int Result { get; set; }
    }

    [Test]
    public async Task SingleInstanceTest()
    {
        ContextHandlers<Object> contextHandlers = new();
        contextHandlers.Register((SumRequest req, Object context) => new SumResponse {Result = req.a + req.b});

        var result = await BlazorWebAssemblySignalRApp.Shared.Rpc.Consumer
            .Send<SumRequest, SumResponse>(new SumRequest {a = 40, b = 2}
                , async (name, payload) =>
                {
                    Console.WriteLine($"name={name}");
                    Console.WriteLine($"payload={payload}");
                    var dispatch = contextHandlers.Dispatch(name, payload, new());
                    return dispatch;
                });
        Assert.AreEqual(42, result.Result);
    }
}

[TestFixture]
public class RpcInterfaceServiceTest
{
    interface ICalculator
    {
        public int Sum(int a, int b);
    }

    class Provider : ICalculator
    {
        public int Sum(int a, int b) => a + b;
    }

    [Test]
    public async Task IntefaceTest()
    {
        Func<string, string, Task<string>> dispatcher = async (name, payload) =>
        {
            return "";
        };
        var result = RpcClient.Create<ICalculator>(dispatcher).Sum(40, 2);
        Assert.AreEqual(42, result);
    }
}

public class RpcClient : DispatchProxy
{
    public Func<string, string, Task<string>> dispatcher;

    public static T Create<T>(Func<string, string, Task<string>> dispatcher)
    {
        var proxy = DispatchProxy.Create<T, RpcClient>();
        var p = proxy as RpcClient;
        p.dispatcher = dispatcher;

        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        Console.WriteLine($"Invoke {targetMethod} {String.Join(", ", args)}");
        var m = targetMethod!;
        var pi = m.GetParameters();
        var name = m.Name;

        var serializedArgs = pi.ToList().Select((p, i) =>
        {
            var str = JsonSerializer.Serialize(args[i], p.ParameterType);
            return str;
        }).ToList();

        var payload = JsonSerializer.Serialize(serializedArgs);
        var result =  dispatcher.Invoke(name, payload);
        Console.WriteLine($"result={result.Result}");

        return 42;
    }
}
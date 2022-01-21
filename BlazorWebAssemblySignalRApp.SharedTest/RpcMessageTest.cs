using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using NUnit.Framework;

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
        contextHandlers.Register((SumRequest req, Object context) => new SumResponse { Result = req.a + req.b });

        var result = await BlazorWebAssemblySignalRApp.Shared.Rpc.Consumer
            .Send<SumRequest, SumResponse>(new SumRequest { a = 40, b = 2 }
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
        public Task<int> Sum(int a, int b);
        public Task<int> Power2(int a);
    }

    class Provider : ICalculator
    {
        public async Task<int> Sum(int a, int b) => a + b;
        public async Task<int> Power2(int a) => a * a;
    }

    [Test]
    public async Task IntefaceTest()
    {
        Func<string, string, Task<string>> dispatcher = async (name, payload) =>
        {
            Console.WriteLine($"dispatcher name={name} payload={payload}");
            var res = JsonSerializer.Serialize(16, typeof(int));
            var restore = JsonSerializer.Deserialize(res, typeof(int));
            return res;
        };
        var power2 = RpcClient.Create<ICalculator>(dispatcher).Power2(4);
        var result = await power2;
        Assert.AreEqual(16, result);
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
        var m = targetMethod!;
        var pi = m.GetParameters();
        var name = m.Name;

        var ser = new MethodSerializer(m);
        var argsSer = ser.ArgsSerializer.Serializer(args[0]);
        var tresult = dispatcher.Invoke(name, argsSer);
        var tresultResult = tresult.Result;
        var r = ser.ReturnSerializer.Deserializer(tresultResult);

        var methodInfo = typeof(Task).GetMethod("FromResult");
        var genericMethodInfo = methodInfo.MakeGenericMethod(ser.ReturnSerializer.Type);
        var result2 = genericMethodInfo.Invoke(null, new object[] { r });
        return result2;

        Console.WriteLine($"Invoke {targetMethod} {String.Join(", ", args)}");

        var serializedArgs = pi.ToList().Select((p, i) =>
        {
            var str = JsonSerializer.Serialize(args[i], p.ParameterType);
            return str;
        }).ToList();

        var payload = JsonSerializer.Serialize(serializedArgs);
        Console.WriteLine($"payload={payload}");
        var result = dispatcher.Invoke(name, payload);
        Console.WriteLine($"result={result.Result}");

        return Task.FromResult(42);
    }
}
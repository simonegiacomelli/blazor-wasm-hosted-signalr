using System;
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
public class RpcTest
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
    public async Task IntegrationTest()
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
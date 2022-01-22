using System;
using System.Threading.Tasks;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using Impl1;
using Interface1;
using NUnit.Framework;

namespace BlazorWebAssemblySignalRApp.SharedTest
{
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
    public class RpcInterfaceMessageTest
    {
        [Test]
        public void test1()
        {
            var msg = RpcInterfaceMessage.Encode("type1", "method1", "bar");
            var restored = RpcInterfaceMessage.Decoder(msg);
            Assert.AreEqual("type1", restored.TypeName);
            Assert.AreEqual("method1", restored.MethodName);
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
        [Test]
        public async Task ServiceWith2ArgsTest()
        {
            var dispatcher = Dispatcher();
            var asyncResult = RpcClient.Create<ICalculator>(dispatcher).Sum(1, 5);
            var result = await asyncResult;
            Assert.AreEqual(6, result);
        }

        [Test]
        public async Task ServiceWith0ArgsTest()
        {
            var dispatcher = Dispatcher();
            var asyncResult = RpcClient.Create<ICalculator>(dispatcher).ZeroArgs();
            var result = await asyncResult;
            Assert.AreEqual(42, result);
        }


        private static Dispatcher Dispatcher()
        {
            var disp = new Registry();
            disp.Register<ICalculator>();

            var dispatcher = disp.Dispatcher((type) =>
            {
                Assert.AreEqual(typeof(ICalculator), type);
                return new Calculator();
            });
            return dispatcher.Invoke;
        }
    }
}

namespace Interface1
{
    interface ICalculator
    {
        public Task<int> Power(int a);
        public Task<int> Sum(int a, int b);
        public Task<int> ZeroArgs();
    }
}


namespace Interface2
{
    interface ICalculator
    {
        public Task<int> Power(int a);
    }
}


namespace Impl1
{
    class Calculator : Interface1.ICalculator
    {
        public async Task<int> Power(int a) => a * a;
        public async Task<int> Sum(int a, int b) => a + b;
        public async Task<int> ZeroArgs() => 42;
    }
}

namespace Impl2
{
    class Calculator : Interface2.ICalculator
    {
        public async Task<int> Power(int a) => a * a * a;
    }
}

[TestFixture]
public class DispatcherTest
{
    [Test]
    public async Task TestGetName()
    {
        var name1 = Registry.GetNameFromType<Interface1.ICalculator>();
        var name2 = Registry.GetNameFromType<Interface2.ICalculator>();

        var dispatcher = new Registry();
        dispatcher.Register<Interface1.ICalculator>();
        dispatcher.Register<Interface2.ICalculator>();

        Assert.AreEqual(typeof(Interface1.ICalculator), dispatcher.GetTypeFromName(name1));
        Assert.AreEqual(typeof(Interface2.ICalculator), dispatcher.GetTypeFromName(name2));
    }
    //
    // [Test]
    // public async Task TestDispatch()
    // {
    //     var calc = new Impl1.Calculator();
    //     Dispatcher.Dispatch<ICalculator>(calc,name,payload);
    // }
}
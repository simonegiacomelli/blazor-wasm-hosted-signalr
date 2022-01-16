using System;
using System.Text.Json;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using NUnit.Framework;

namespace BlazorWebAssemblySignalRApp.SharedTest;

public class MarshalTest
{
    [Test]
    public void WithoutMarshaling()
    {
        IService1 provider = new Provider();
        var consumer = new Consumer { Service1 = provider };
        var result = consumer.Consume(42);
        Assert.AreEqual("Consuming echo 42", result);
    }


    [Test]
    public void PlayWithSerialization()
    {
        var dt = new DateTime(1977, 1, 18, 23, 58, 59);
        var request = new Request { Name = "foo", Age = 42, Delivery = dt };

        string jsonString = JsonSerializer.Serialize(request);
        Console.WriteLine(jsonString);
        var r = JsonSerializer.Deserialize<Request>(jsonString)!;

        Assert.AreEqual("foo", r.Name);
        Assert.AreEqual(42, r.Age);
        Assert.AreEqual(dt, r.Delivery);
    }

    [Test]
    public void PlayWithSerialization2()
    {
        var dt = new DateTime(1977, 1, 18, 23, 58, 59);
        var request = new Request { Name = "foo", Age = 42, Delivery = dt };

        var ser = AnySerializer.New<Request>();
        string jsonString = ser.Serializer(request);
        Console.WriteLine(jsonString);
        var r = ser.Deserializer(jsonString) as Request;

        Assert.AreEqual("foo", r.Name);
        Assert.AreEqual(42, r.Age);
        Assert.AreEqual(dt, r.Delivery);
    }
}

class Request
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public DateTime Delivery { get; set; }
}

class Response
{
    public Object Result;
}

interface IService1
{
    String Echo(int value);
}

class Provider : IService1
{
    public string Echo(int value) => $"echo {value}";
}

class Consumer
{
    public IService1 Service1;

    public String Consume(int value) => "Consuming " + Service1.Echo(value);
}
using System;
using System.Collections;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Security.Permissions;
using BlazorWebAssemblySignalRApp.Shared;
using NUnit.Framework;

namespace BlazorWebAssemblySignalRApp.SharedTest;

public class Tests
{

    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var p = DispatchProxy.Create<IWeatherForecastService, MyProxy>();
        var res = p.Echo("simo", 44);
        Console.WriteLine(res);
    }

    [Test]
    public void Test2()
    {
        var target = new Target<IMockService>();
        target.Inspect();
    }

    class Target<T>
    {
        public Target()
        {
        }

        public void Inspect()
        {
            typeof(T)
                .GetMembers()
                .OfType<MethodInfo>()
                .ToList()
                .ForEach(Inspect);
        }

        private void Inspect(MethodInfo mi)
        {
            Console.WriteLine(mi);
        }
    }
    
    
    interface IMockService
    {
        string Service1(string arg1, long arg2);
        (string, DateTime) Service2();
    }

    public class MyProxy : DispatchProxy
    {
        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            Console.WriteLine($"Invoke {targetMethod} {String.Join(", ", args)}");
            return "Done";
        }
    }
}

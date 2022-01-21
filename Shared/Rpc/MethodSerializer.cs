using System.Reflection;

namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class MethodSerializer
{
    public MethodSerializer(MethodInfo method)
    {
        //todo deal with Result type Task<T>
        //one should not serializer the instance of Task<T> 
        //but unwrap it 
        var ret = method.ReturnType.GenericTypeArguments[0];
        ReturnSerializer = AnySerializer.New(ret);

        var arg0 = method.GetParameters()[0].ParameterType;
        ArgsSerializer = AnySerializer.New(arg0);
    }

    public AnySerializer ArgsSerializer { get; set; }
    public AnySerializer ReturnSerializer { get; set; }
}
using System.Reflection;
using System.Text.Json;

namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class MethodSerializer
{
    public AnySerializer ArgsSerializer { get; set; }
    public AnySerializer ReturnSerializer { get; set; }

    public MethodSerializer(MethodInfo method)
    {
        //todo deal with Result type Task<T>
        //one should not serializer the instance of Task<T> 
        //but unwrap it 
        var ret = method.ReturnType.GenericTypeArguments[0];
        ReturnSerializer = AnySerializer.New(ret);

        var argSerializer = method.GetParameters().Select(p => AnySerializer.New(p.ParameterType)).ToList();

        ArgsSerializer = new AnySerializer
        {
            Serializer = o =>
            {
                var o2 = (object?[])o;
                var o3 = argSerializer.Select((s, i) => s.Serializer(o2[i]));
                return JsonSerializer.Serialize(o3);
            },
            Deserializer = s =>
            {
                var o3 = JsonSerializer.Deserialize<IEnumerable<String>>(s).ToList();
                var o2 = argSerializer.Select((s, i) => s.Deserializer(o3[i]));
                var o = o2.ToArray();
                return o;
            }
        };
    }
}
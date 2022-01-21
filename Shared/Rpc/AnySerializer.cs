using System.Text.Json;

namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class AnySerializer
{
    public static AnySerializer New<T>() where T : class
    {
        return new AnySerializer
        {
            Serializer = o => JsonSerializer.Serialize<T>((T)o),
            Deserializer = s => JsonSerializer.Deserialize<T>(s)!
        };
    }

    public static AnySerializer New(Type type)
    {
        return new AnySerializer
        {
            Type = type,
            Serializer = o => JsonSerializer.Serialize(o, type),
            Deserializer = s => JsonSerializer.Deserialize(s, type)!
        };
    }

    public Type Type { get; set; }
    public Func<Object, string> Serializer { get; set; }
    public Func<string, Object> Deserializer { get; set; }
}
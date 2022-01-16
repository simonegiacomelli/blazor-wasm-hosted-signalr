using System.Text.Json;

namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public interface IRequest<TRes>
{
}

class ContextHandler<TContext>
{
    public AnySerializer RequestSerializer;
    public AnySerializer ResponseSerializer;
    public Delegate Handler;
}

public class ContextHandlers<TContext>
{
    private Dictionary<String, ContextHandler<TContext>> _contextHandlers = new();

    public void Register<TReq, TRes>(Func<TReq, TContext, TRes> function)
        where TReq : class where TRes : class
    {
        // CheckSerializable<TReq>();
        // CheckSerializable<TRes>();

        var handlerName = typeof(TReq).FullName!;
        var contextHandler = new ContextHandler<TContext>
        {
            RequestSerializer = AnySerializer.New<TReq>(),
            ResponseSerializer = AnySerializer.New<TRes>(),
            Handler = function
        };
        _contextHandlers[handlerName] = contextHandler;
    }

    private void CheckSerializable<T>() where T : class
    {
        var type = typeof(T);
        if (!type.IsSerializable)
            throw new ArgumentException($"Class `{type.FullName}` is not serializable");
    }

    public String Dispatch(RpcMessage m, TContext c) => Dispatch(m.Name, m.Payload, c);

    public String Dispatch(string name, string payload, TContext context)
    {
        var handler = _contextHandlers.GetValueOrDefault(name) ??
                      throw new Exception($"no handler registered for `{name}`");
        var r = handler.RequestSerializer.Deserializer(payload);
        var res = handler.Handler.Method.Invoke(handler.Handler.Target
            , new object[] { r, context });
        var ser = handler.ResponseSerializer.Serializer(res);
        return ser;
    }
}

public class Consumer
{
    public static async Task<TRes> Send<TReq, TRes>(TReq request, Func<string, string, Task<string>> dispatcher)
        where TReq : class, IRequest<TRes> where TRes : class
    {
        var requestSerializer = AnySerializer.New<TReq>();
        var responseSerializer = AnySerializer.New<TRes>();
        var requestSerialized = requestSerializer.Serializer(request);
        var responseSerialized = await dispatcher(typeof(TReq).FullName!, requestSerialized);
        var response = responseSerializer.Deserializer(responseSerialized);
        return response as TRes;
    }
}

public class RpcMessage
{
    public string Name;
    public string Payload;
    public const string HandlerName = "/rpc-api";

    private const string Marker = "#dotnet";

    public static RpcMessage Decoder(string message)
    {
        var (header, payload, _) = message.Split("\n\n", count: 2);
        var props = header.Split("\n").ToList()
            .FindAll(e => e.Trim().Length > 0)
            .Select(e =>
            {
                var (k, v, _) = e.Split("=", count: 2);
                return (k, v);
            })
            .ToDictionary(e => e.Item1, e => e.Item2);
        var name = props.GetValueOrDefault("name")??throw new Exception($"key name not found in message ```{message}```");
        return new RpcMessage { Name = name, Payload = payload };
    }


    public static string Encode(string name, string payload)
    {
        var message = $"name={name}\n\n{payload}";
        return message;
    }
}

public static class Extensions
{
    public static void Deconstruct<T>(this IList<T> list, out T first, out IList<T> rest)
    {
        first = list.Count > 0 ? list[0] : default(T); // or throw
        rest = list.Skip(1).ToList();
    }

    public static void Deconstruct<T>(this IList<T> list, out T first, out T second, out IList<T> rest)
    {
        first = list.Count > 0 ? list[0] : default(T); // or throw
        second = list.Count > 1 ? list[1] : default(T); // or throw
        rest = list.Skip(2).ToList();
    }
}

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

    public Func<Object, string> Serializer { get; set; }
    public Func<string, Object> Deserializer { get; set; }
}
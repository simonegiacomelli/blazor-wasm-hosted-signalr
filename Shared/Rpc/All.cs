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
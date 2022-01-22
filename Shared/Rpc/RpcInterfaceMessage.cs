namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class RpcInterfaceMessage
{
    public string TypeName;
    public string MethodName;
    public string Payload;


    public RpcInterfaceMessage(string typeName, string methodName, string payload)
    {
        TypeName = typeName;
        MethodName = methodName;
        Payload = payload;
    }


    public const string HandlerName = "/rpc-interface-api";

    private const string Marker = "#dotnet-interface";

    public static RpcInterfaceMessage Decoder(string message)
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

        string GetProp(string key) => props.GetValueOrDefault(key) ??
                                      throw new Exception($"key `{key}` not found in message ```{message}```");

        return new RpcInterfaceMessage(GetProp("type"), GetProp("method"), payload);
    }


    public static string Encode(string typeName, string methodName, string payload)
    {
        var message = $"type={typeName}\nmethod={methodName}\n\n{payload}";
        return message;
    }
}
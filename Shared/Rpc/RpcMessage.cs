namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

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
        var name = props.GetValueOrDefault("name") ??
                   throw new Exception($"key name not found in message ```{message}```");
        return new RpcMessage { Name = name, Payload = payload };
    }


    public static string Encode(string name, string payload)
    {
        var message = $"name={name}\n\n{payload}";
        return message;
    }
}
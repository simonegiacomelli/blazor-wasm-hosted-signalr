namespace BlazorWebAssemblySignalRApp.Shared.Rpc;

public class EchoRequest : IRequest<EchoResponse>
{
    public string Str { get; set; }
}

public class EchoResponse
{
    public string Result { get; set; }
}
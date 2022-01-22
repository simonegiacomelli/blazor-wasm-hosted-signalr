using BlazorWebAssemblySignalRApp.Shared.Rpc;
using Microsoft.AspNetCore.Mvc;

namespace BlazorWebAssemblySignalRApp.Server.Controllers;

public class RpcInterfaceController
{
    [Consumes("text/plain")]
    [HttpPost(RpcInterfaceMessage.HandlerName)]
    public async Task<String> Handler([FromBody] string content, [FromServices] Registry registry,
        [FromServices] IServiceProvider serviceProvider)
    {
        var body = content;
        Console.WriteLine($"RpcInterfaceMessage got it {body}");
        var msg = RpcInterfaceMessage.Decoder(body);
        var dispatcher = registry.Dispatcher(type => serviceProvider.GetService(type)!);
        var result = await dispatcher(msg);
        return result;
    }
}
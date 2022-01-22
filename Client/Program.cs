using System.Net.Http.Json;
using BlazorWebAssemblySignalRApp.Client;
using BlazorWebAssemblySignalRApp.Shared;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

Console.WriteLine("ciao");

Dispatcher dispatcher = async (RpcInterfaceMessage msg) =>
{
    var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
    var response = await http.PostAsJsonAsync(RpcInterfaceMessage.HandlerName, msg);
    var res = response.Content.ReadAsStringAsync();
    return await res;
};
var result = await RpcClient.Create<IRpcTest>(dispatcher).Sum(1, 5);
Console.WriteLine($"Result from server {result}");

await builder.Build().RunAsync();
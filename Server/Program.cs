using System.Text;
using System.Text.Json;
using BlazorWebAssemblySignalRApp.Server.Hubs;
using BlazorWebAssemblySignalRApp.Shared;
using BlazorWebAssemblySignalRApp.Shared.Rpc;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


// signalR
builder.Services.AddSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});
// signalR

var app = builder.Build();

app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();


app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapFallbackToFile("index.html");

var contextHandler = new ContextHandlers<Object>();
contextHandler.Register(
    (EchoRequest req, Object context) => { return new EchoResponse { Result = $"echo {req.Str}" }; });
app.MapPost(RpcMessage.HandlerName, async r =>
{
    var stream = new StreamReader(r.Request.Body);
    var body = await stream.ReadToEndAsync();
    Console.WriteLine($"RpcMessage got it {body}");
    var res = contextHandler.Dispatch(RpcMessage.Decoder(body), new());
    var bytes = Encoding.UTF8.GetBytes(res);
    await r.Response.Body.WriteAsync(bytes, 0, bytes.Length);
});

var registry = new Registry();
registry.Register<IRpcTest>();

app.MapPost(RpcInterfaceMessage.HandlerName, async r =>
{
    var stream = new StreamReader(r.Request.Body);
    var body = await stream.ReadToEndAsync();
    Console.WriteLine($"RpcInterfaceMessage got it {body}");
    var msg = JsonSerializer.Deserialize<RpcInterfaceMessage>(body);

    var dispatcher = registry.Dispatcher(type => new RpcTest());
    var result = await dispatcher(msg);
    await r.Response.WriteAsync(result);
});
app.Run();
namespace BlazorWebAssemblySignalRApp.Shared;

interface IRpcTest
{
    Task<int> Sum(int a, int b);
}

class RpcTest : IRpcTest
{
    public async Task<int> Sum(int a, int b) => a + b;
}

public class Api
{
}
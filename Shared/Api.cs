namespace BlazorWebAssemblySignalRApp.Shared;

public interface IRpcTest
{
    Task<int> Sum(int a, int b);
}

public class RpcTest : IRpcTest
{
    public async Task<int> Sum(int a, int b) => a + b;
}

public class Api
{
}
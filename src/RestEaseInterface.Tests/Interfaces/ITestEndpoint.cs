using RestEase;
using RestEaseInterface.Tests.Entities;

namespace RestEaseInterface.Tests.Interfaces; 

public interface ITestEndpoint {
    [Get("test")]
    Task TestMe();

    [Post("echo")]
    Task<string> Echo(string echoMessage);
    
    [Post("division")]
    Task<double> Division(int numberOne, int numberTwo);

    [Get("entity")]
    Task<TestEntity> GetEntity(string name, string description);

    [Get("stream")]
    Task<Stream> GetStream(string streamMessage);
}
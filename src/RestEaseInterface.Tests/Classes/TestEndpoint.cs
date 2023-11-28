using RestEaseInterface.Tests.Entities;
using RestEaseInterface.Tests.Interfaces;

namespace RestEaseInterface.Tests.Classes; 

public class TestEndpoint : ITestEndpoint {
    public async Task TestMe() {
        //blank
    }

    public async Task<string> Echo(string echoMessage) {
        return echoMessage;
    }

    public async Task<double> Division(int numberOne, int numberTwo) {
        return numberOne / (double)numberTwo;
    }

    public async Task<TestEntity> GetEntity(string name, string description) {
        return new TestEntity {
            Name = name,
            Description = description
        };
    }

    public async Task<Stream> GetStream(string streamMessage) {
        MemoryStream stream = new MemoryStream();
        StreamWriter writer = new StreamWriter(stream);
        await writer.WriteAsync(streamMessage);
        await writer.FlushAsync();
        stream.Position = 0;
        return stream;
    }
}